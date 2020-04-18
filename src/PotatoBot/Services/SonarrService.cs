﻿using PotatoBot.API;
using PotatoBot.Modals;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.Requests.POST;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PotatoBot.Services
{
    internal class SonarrService : APIBase, IService
    {
        public string Name => "Sonarr";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // Cache things

        private readonly object _seriesCacheLock = new object();
        private DateTime _seriesCacheUpdates = DateTime.MinValue;
        // SeriesId -> Series
        private Dictionary<ulong, Series> _seriesCache = new Dictionary<ulong, Series>();
        private readonly object _episodeCacheLock = new object();
        // SeriesId -> EpisodeId -> Episode
        private readonly Dictionary<ulong, Dictionary<ulong, Episode>> _episodeCache = new Dictionary<ulong, Dictionary<ulong, Episode>>();

        internal SonarrService(EntertainmentSettings settings, string apiUrl) : base(settings, apiUrl)
        {
            var systemStatus = GetSystemStatus();
            _logger.Trace("======= System Info =======");
            _logger.Trace($"Version: {systemStatus.Version}");
            _logger.Trace($"OS: {systemStatus.OSVersion}");
            _logger.Trace($"IsMono: {systemStatus.IsMono}");
            _logger.Trace($"IsLinux: {systemStatus.IsLinux}");
            _logger.Trace($"IsWindows: {systemStatus.IsWindows}");
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        internal List<Series> GetSeries()
        {
            lock (_seriesCacheLock)
            {
                if (_seriesCache.Count == 0 || _seriesCacheUpdates.AddMinutes(30) < DateTime.Now)
                {
                    _logger.Trace("Updating Series cache ...");
                    _seriesCache = GetRequest<List<Series>>(APIEndPoints.Sonarr.Series).ToDictionary((i) => i.Id);
                }
            }

            return _seriesCache.Values.ToList();
        }

        internal List<Series> SearchSeries(string name)
        {
            _logger.Trace($"Searching for series with name '{name}' ...");
            var body = new LookupRequest()
            {
                SearchTerm = name
            };

            var response = GetRequest<List<Series>>(APIEndPoints.Sonarr.Lookup, body);
            _logger.Trace($"Got {response?.Count ?? 0} series as response");

            return response;
        }

        internal AddResult AddSeries(Series series)
        {
            _logger.Trace($"Adding series [{series.TvDbId}] {series.Title}");

            var body = new AddSeries(series);

            var response = PostRequest<Series>(APIEndPoints.Sonarr.Series, body, System.Net.HttpStatusCode.Created);
            if(response.Item1 != null)
            {
                var seriesResult = response.Item1;
                _logger.Trace($"Successfully added {seriesResult.Title} ({seriesResult.Path})");
            }

            return new AddResult
            {
                Added = response.Item1 != null,
                AlreadyAdded = response.Item2 == System.Net.HttpStatusCode.BadRequest, // BadRequest = Series alread exists (well could be other things too but idc)
                StatusCode = response.Item2
            };
        }

        internal virtual List<SonarrQueueItem> GetQueue()
        {
            _logger.Trace("Fetching download queue");

            var response = GetRequest<Modals.API.Queue<SonarrQueueItem>>(APIEndPoints.Queue, new QueueRequest());
            if (response != null)
            {
                _logger.Trace("Successfully fetched download queue");
                return response.Records;
            }
            return null;
        }

        internal Series GetSeriesInfo(uint seriesId)
        {
            _logger.Trace($"Getting series info for series {seriesId}");

            lock (_seriesCacheLock)
            {
                if (_seriesCache.ContainsKey(seriesId))
                {
                    _logger.Trace($"Fetched series from cache");
                    return _seriesCache[seriesId];
                }

                var endpoint = $"{APIEndPoints.Sonarr.Series}/{seriesId}";

                var response = GetRequest<Series>(endpoint);
                if (response != null)
                {
                    _seriesCache.Add(seriesId, response);

                    _logger.Trace($"Successfully fetched series");
                }
                return response;
            }
        }

        internal Episode GetEpisodeInfo(ulong seriesId, ulong episodeId)
        {
            _logger.Trace($"Getting episode info for series {seriesId} episode {episodeId}");

            lock (_episodeCacheLock)
            {
                if (_episodeCache.ContainsKey(seriesId) && _episodeCache[seriesId].ContainsKey(episodeId))
                {
                    _logger.Trace($"Fetched episode from cache");
                    return _episodeCache[seriesId][episodeId];
                }

                var endpoint = string.Format(APIEndPoints.Sonarr.Episode, episodeId);
                var requestBody = new RequestEpisode()
                {
                    SeriesId = seriesId
                };

                var response = GetRequest<Episode>(endpoint, requestBody);
                if (response != null)
                {
                    if (!_episodeCache.ContainsKey(seriesId))
                    {
                        _episodeCache.Add(seriesId, new Dictionary<ulong, Episode>());
                    }
                    _episodeCache[seriesId].Add(episodeId, response);

                    _logger.Trace($"Successfully fetched episode info");
                }
                return response;
            }
        }
    }
}