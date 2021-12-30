using PotatoBot.API;
using PotatoBot.Modals;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.Requests.POST;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;

namespace PotatoBot.Services
{
    public class SonarrService : APIBase, IService, IServarr
    {
        public ServarrType Type => ServarrType.Sonarr;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // Cache things

        private readonly object _seriesCacheLock = new();
        // SeriesId -> Series
        private readonly Dictionary<ulong, Series> _seriesCache = new();
        private readonly object _episodeCacheLock = new();
        // SeriesId -> EpisodeId -> Episode
        private readonly Dictionary<ulong, Dictionary<ulong, Episode>> _episodeCache = new();

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

        public IEnumerable<IServarrItem> GetAll()
        {
            _logger.Trace("Fetching all series ...");

            var response = GetRequest<List<Series>>(APIEndPoints.SonarrEndpoints.Series);
            _logger.Trace($"Got {response.Count} series as a response");

            return response;
        }

        public IEnumerable<IServarrItem> Search(string name)
        {
            _logger.Trace($"Searching for series with name '{name}' ...");
            var body = new LookupRequest()
            {
                SearchTerm = name
            };

            var response = GetRequest<List<Series>>(APIEndPoints.SonarrEndpoints.Lookup, body);
            _logger.Trace($"Got {response?.Count ?? 0} series as response");

            return response;
        }

        public AddResult Add(IServarrItem item)
        {
            var series = item as Series ?? throw new ArgumentNullException(nameof(item));

            _logger.Trace($"Adding series [{series.TvDbId}] {series.Title}");

            var body = new AddSeries(this, series);

            var response = PostRequest<Series>(APIEndPoints.SonarrEndpoints.Series, body, System.Net.HttpStatusCode.Created);
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

        public override List<QueueItem> GetQueue()
        {
            _logger.Trace("Fetching download queue");

            var response = GetRequest<Modals.API.Queue<SonarrQueueItem>>(APIEndPoints.Queue, new QueueRequest());
            if(response != null)
            {
                _logger.Trace("Successfully fetched download queue");
                var list = new List<QueueItem>();
                foreach(var record in response.Records)
                {
                    record.API = this;
                    list.Add(record);
                }
                return list;
            }
            return null;
        }

        internal Series GetSeriesInfo(uint seriesId)
        {
            _logger.Trace($"Getting series info for series {seriesId}");

            lock(_seriesCacheLock)
            {
                if(_seriesCache.ContainsKey(seriesId))
                {
                    _logger.Trace($"Fetched series from cache");
                    return _seriesCache[seriesId];
                }

                var endpoint = $"{APIEndPoints.SonarrEndpoints.Series}/{seriesId}";

                var response = GetRequest<Series>(endpoint);
                if(response != null)
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

            lock(_episodeCacheLock)
            {
                if(_episodeCache.ContainsKey(seriesId) && _episodeCache[seriesId].ContainsKey(episodeId))
                {
                    _logger.Trace($"Fetched episode from cache");
                    return _episodeCache[seriesId][episodeId];
                }

                var endpoint = string.Format(APIEndPoints.SonarrEndpoints.Episode, episodeId);
                var requestBody = new RequestEpisode()
                {
                    SeriesId = seriesId
                };

                var response = GetRequest<Episode>(endpoint, requestBody);
                if(response != null)
                {
                    if(!_episodeCache.ContainsKey(seriesId))
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
