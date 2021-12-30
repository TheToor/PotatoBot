using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Plex;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoBot.Services
{
    public class WebhookService : IService
	{
		public string Name => "Webhook Endpoint";

		private static Modals.Settings.WebhookSettings _settings => Program.Settings.Webhook;

		private IWebHost _endpoint;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly System.Timers.Timer _cacheUpdateTimer;

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		internal WebhookService()
		{
			_cacheUpdateTimer = new System.Timers.Timer(1000 * 60 * 60 * 24);
			_cacheUpdateTimer.Elapsed += UpdateCache;
			_cacheUpdateTimer.AutoReset = true;
		}

		private void UpdateCache(object sender, System.Timers.ElapsedEventArgs e)
		{
			_logger.Trace("Updating Media Preview cache ...");

			if(Program.Settings.DebugNoPreview)
			{
				_logger.Warn("Not updating Preview cache due to debug setting");
				return;
			}

			{
				var plexServers = Program.ServiceManager.GetPlexServices();

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

				API.Preview.CachedStatisticsResponse = response;
			}

			_logger.Info("Finished updating Media Preview cache");

			_logger.Debug("Updating Preview Cache ...");

			{
				var cachedResponse = new Dictionary<string, object>();

				if(Program.Settings.Radarr.Count > 0)
				{
					var movies = Program.ServiceManager.Radarr.SelectMany(r => r.GetAll()).Distinct().ToList();
					if(movies != null)
					{
						cachedResponse.Add("Movies", movies.ConvertAll((o) => new BasicMovie(o)));
					}
				}

				if(Program.Settings.Sonarr.Count > 0)
				{
					var series = Program.ServiceManager.Sonarr.SelectMany(s => s.GetAll()).Distinct().ToList();
					if(series != null)
					{
						cachedResponse.Add("Series", series.ConvertAll((o) => new BasicSeries(o)));
					}
				}

				if(Program.Settings.Lidarr.Count > 0)
				{
					var artists = Program.ServiceManager.Lidarr.SelectMany(l => l.GetAll()).Distinct().ToList();
					if(artists != null)
					{
						cachedResponse.Add("Artists", artists.ConvertAll((o) => new BasicArtist(o)));
					}
				}

				API.Preview.CachedPreviewResponse = cachedResponse;
			}

			_logger.Info("Finished updating Preview cache");
		}

		public bool Start()
		{
			try
			{
				var config = new ConfigurationBuilder()
					.SetBasePath(System.IO.Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
					.Build();

				_endpoint = new WebHostBuilder()
					.UseConfiguration(config)
					.UseKestrel()
					.ConfigureLogging((logging) =>
					{
						logging.ClearProviders();
#if WEB_DEBUG
                        logging.SetMinimumLevel(LogLevel.Trace);
#else
						logging.SetMinimumLevel(LogLevel.Warning);
#endif
					})
					.UseNLog()
					.UseStartup<Startup>()
					.UseUrls(_settings.BindingUrl)
					.SuppressStatusMessages(true)
					.Build();

				_endpoint.RunAsync(_cancellationTokenSource.Token);

				_cacheUpdateTimer.Start();

				Task.Factory.StartNew(async () =>
				{
					// Wait some time until the Program is initialized
					await Task.Delay(5000);

					// Update cache one manually
					UpdateCache(null, null);
				});

				_logger.Info($"Started {Name} on '{_settings.BindingUrl}'");
				return true;
			}
			catch(Exception ex)
			{
				_logger.Error(ex, $"Failed to start {Name}");
				return false;
			}
		}

		public bool Stop()
		{
			_logger.Info($"Received Stop signal for {Name}");

			try
			{
				if(!_cancellationTokenSource.IsCancellationRequested)
				{
					_logger.Trace("Requesting cancellation ...");
					_cancellationTokenSource.Cancel();
					_logger.Trace("Cleaning up ...");
					_endpoint.Dispose();
				}

				_cacheUpdateTimer.Stop();
				_cacheUpdateTimer.Dispose();

				_cancellationTokenSource.Dispose();
			}
			catch(Exception ex)
			{
				_logger.Warn(ex, $"Failed to correctly stop {Name}");
			}

			return true;
		}
	}
}
