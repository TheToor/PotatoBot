using PotatoBot.API;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PotatoBot.Services
{
    internal class SonarrService : APIBase, IService
    {
        public string Name => "Sonarr";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private DateTime _seriesCacheUpdates = DateTime.MinValue;
        private List<Series> _seriesCache = new List<Series>();

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
            if(_seriesCache.Count == 0 || _seriesCacheUpdates.AddMinutes(30) < DateTime.Now)
            {
                _logger.Trace("Updating Series cache ...");
                _seriesCache = GetRequest<List<Series>>(APIEndPoints.Sonarr.Series);
            }

            return _seriesCache;
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
    }
}
