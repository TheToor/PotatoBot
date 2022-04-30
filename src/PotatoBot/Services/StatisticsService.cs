using Newtonsoft.Json;
using PotatoBot.Model;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PotatoBot.Services
{
    public class StatisticsService : IDisposable
    {
        public string Name => "Statistics";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string _statisticsFileName = "stats.json";

        private Statistics? _statistics;

        // Delay between saves
        // This prevents spam saving the file to the disk
        private static readonly int _saveDelay = 600;
        private DateTime _lastSaved = DateTime.MinValue;

        public StatisticsService()
        {
            LoadStatistics().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            SaveStatistics().ConfigureAwait(false).GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        private async Task LoadStatistics()
        {
            if(!File.Exists(_statisticsFileName))
            {
                // if no statistics have yet been saved then create an empty file
                _statistics = new Statistics();
                await SaveStatistics();
            }

            var statistics = await File.ReadAllTextAsync(_statisticsFileName);
            if(string.IsNullOrEmpty(statistics))
            {
                _logger.Error("Invalid statistics file");
                return;
            }

            try
            {
                _statistics = JsonConvert.DeserializeObject<Statistics>(statistics);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Invalid settings file contents");
                return;
            }

            _logger.Info("Successfully read statistics");
            return;
        }

        private async Task TrySave()
        {
            if(DateTime.Now < _lastSaved.AddSeconds(_saveDelay))
            {
                // Do not save when last save is not longer than _saveDelay seconds ago
                _logger.Trace($"NOT executing save as last save was on {_lastSaved} ({_saveDelay} / {DateTime.Now})");
                return;
            }

            // Else just execute the normal save
            await SaveStatistics();
        }

        private async Task SaveStatistics()
        {
            try
            {
                if(File.Exists(_statisticsFileName))
                {
                    File.Delete(_statisticsFileName);
                }

                await File.WriteAllTextAsync(_statisticsFileName, JsonConvert.SerializeObject(_statistics));

                _lastSaved = DateTime.Now;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to save statistics");
            }
        }

        public async Task Increase(TrackedStatistics statistics)
        {
            if(_statistics == null)
            {
                return;
            }

            switch(statistics)
            {
                case TrackedStatistics.MessagesSent:
                    _statistics.MessagesSent++;
                    break;
                case TrackedStatistics.MessagesReceived:
                    _statistics.MessagesReveived++;
                    break;
                case TrackedStatistics.MessagesProcessed:
                    _statistics.MessagesProcessed++;
                    break;
                case TrackedStatistics.CommandsReceived:
                    _statistics.CommandsReceived++;
                    break;
                case TrackedStatistics.CommandsProcessed:
                    _statistics.CommandsProcessed++;
                    break;
                case TrackedStatistics.Searches:
                    _statistics.Searches++;
                    break;
                case TrackedStatistics.Adds:
                    _statistics.Adds++;
                    break;
                case TrackedStatistics.WebhooksReceived:
                    _statistics.WebhooksReceived++;
                    break;
                case TrackedStatistics.WebhooksProcessed:
                    _statistics.WebhooksProcessed++;
                    break;
            }

            await TrySave();
        }

        internal Statistics? GetStatistics()
        {
            return _statistics;
        }       
    }
}
