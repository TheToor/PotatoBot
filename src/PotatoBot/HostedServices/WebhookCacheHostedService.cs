﻿using Microsoft.Extensions.Hosting;
using PotatoBot.Model.API.Lidarr;
using PotatoBot.Model.API.Plex;
using PotatoBot.Model.API.Radarr;
using PotatoBot.Model.API.Sonarr;
using PotatoBot.Model.Settings;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoBot.HostedServices
{
    public class WebhookCacheHostedService : IHostedService
    {
        public string Name => "Webhook Endpoint";

        private readonly System.Timers.Timer _cacheUpdateTimer;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ServiceManager _serviceManager;
        private readonly BotSettings _botSettings;

        public WebhookCacheHostedService(ServiceManager serviceManager, BotSettings botSettings)
        {
            _serviceManager = serviceManager;
            _botSettings = botSettings;

            _cacheUpdateTimer = new System.Timers.Timer(1000 * 60 * 60 * 24);
            _cacheUpdateTimer.Elapsed += UpdateCache;
            _cacheUpdateTimer.AutoReset = true;
        }

        private void UpdateCache(object? sender, System.Timers.ElapsedEventArgs? e)
        {
            _logger.Trace("Updating Media Preview cache ...");

            if(_botSettings.DebugNoPreview)
            {
                _logger.Warn("Not updating Preview cache due to debug setting");
                return;
            }

            {
                var plexServers = _serviceManager.GetPlexServices();

                var response = new Dictionary<string, ulong>()
                {
                    { "Movies", 0 },
                    { "Series", 0 },
                    { "Music", 0 }
                };

                foreach(var plexServer in plexServers)
                {
                    var stats = plexServer.GetMediaStatistics();

                    foreach(var media in stats.StatisticsMedia)
                    {
                        if(!Enum.TryParse(media.MetaDataType.ToString(), out MediaType mediaType))
                        {
                            _logger.Warn($"Failed to convert '{media.MetaDataType}' to a valid MediaType");
                            continue;
                        }

                        switch(mediaType)
                        {
                            case MediaType.Movie:
                                response["Movies"] += media.Duration;
                                break;

                            case MediaType.Episode:
                                response["Series"] += media.Duration;
                                break;

                            case MediaType.Track:
                                response["Music"] += media.Duration;
                                break;
                        }
                    }
                }

                Controllers.Preview.CachedStatisticsResponse = response;
            }

            _logger.Info("Finished updating Media Preview cache");

            _logger.Debug("Updating Preview Cache ...");

            {
                var cachedResponse = new Dictionary<string, object>();

                if(_botSettings.Radarr.Count > 0)
                {
                    var movies = _serviceManager.Radarr.SelectMany(r => r.GetAll()).Distinct().ToList();
                    if(movies != null)
                    {
                        cachedResponse.Add("Movies", movies.ConvertAll((o) => new BasicMovie(o)));
                    }
                }

                if(_botSettings.Sonarr.Count > 0)
                {
                    var series = _serviceManager.Sonarr.SelectMany(s => s.GetAll()).Distinct().ToList();
                    if(series != null)
                    {
                        cachedResponse.Add("Series", series.ConvertAll((o) => new BasicSeries(o)));
                    }
                }

                if(_botSettings.Lidarr.Count > 0)
                {
                    var artists = _serviceManager.Lidarr.SelectMany(l => l.GetAll()).Distinct().ToList();
                    if(artists != null)
                    {
                        cachedResponse.Add("Artists", artists.ConvertAll((o) => new BasicArtist(o)));
                    }
                }

                Controllers.Preview.CachedPreviewResponse = cachedResponse;
            }

            _logger.Info("Finished updating Preview cache");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cacheUpdateTimer.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(async () =>
                {
                    // Wait some time until the Program is initialized
                    await Task.Delay(5000);

                    // Update cache one manually
                    UpdateCache(null, null);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to start {Name}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"Received Stop signal for {Name}");

            try
            {
                _cacheUpdateTimer.Stop();
                _cacheUpdateTimer.Dispose();
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"Failed to correctly stop {Name}");
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
