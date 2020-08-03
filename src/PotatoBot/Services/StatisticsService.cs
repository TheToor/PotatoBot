using Newtonsoft.Json;
using PotatoBot.Modals;
using System;
using System.IO;

namespace PotatoBot.Services
{
    public class StatisticsService : IService
    {
        public string Name => "Statistics";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static string _statisticsFileName = "stats.json";

        private static Statistics _statistics;

        // Delay between saves
        // This prevents spam saving the file to the disk
        private static int _saveDelay = 600;
        private DateTime _lastSaved = DateTime.MinValue;

        public bool Start()
        {
            LoadStatistics();
            return true;
        }

        public bool Stop()
        {
            SaveStatistics();
            return true;
        }

        private void LoadStatistics()
        {
            if (!File.Exists(_statisticsFileName))
            {
                // if no statistics have yet been saved then create an empty file
                _statistics = new Statistics();
                SaveStatistics();
            }

            var statistics = File.ReadAllText(_statisticsFileName);
            if (string.IsNullOrEmpty(statistics))
            {
                _logger.Error("Invalid statistics file");
                return;
            }

            try
            {
                _statistics = JsonConvert.DeserializeObject<Statistics>(statistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Invalid settings file contents");
                return;
            }

            _logger.Info("Successfully read statistics");
            return;
        }

        private void TrySave()
        {
            if(DateTime.Now < _lastSaved.AddSeconds(_saveDelay))
            {
                // Do not save when last save is not longer than _saveDelay seconds ago
                _logger.Trace($"NOT executing save as last save was on {_lastSaved} ({_saveDelay} / {DateTime.Now})");
                return;
            }

            // Else just execute the normal save
            SaveStatistics();
        }

        private void SaveStatistics()
        {
            try
            {
                if (File.Exists(_statisticsFileName))
                {
                    File.Delete(_statisticsFileName);
                }

                File.WriteAllText(_statisticsFileName, JsonConvert.SerializeObject(_statistics));

                _lastSaved = DateTime.Now;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to save statistics");
            }
        }

        
        internal Statistics GetCurrentStatistics()
        {
            return _statistics;
        }

        public void IncreaseMessagesSent()
        {
            _statistics.MessagesSent++;
            TrySave();
        }

        public void IncreaseMessagesReveived()
        {
            _statistics.MessagesReveived++;
            TrySave();
        }

        public void IncreaseMessagesProcessed()
        {
            _statistics.MessagesProcessed++;
            TrySave();
        }

        public void IncreaseCommandsReceived()
        {
            _statistics.CommandsReceived++;
            TrySave();
        }

        public void IncreaseCommandsProcessed()
        {
            _statistics.CommandsProcessed++;
            TrySave();
        }

        public void IncreaseSearches()
        {
            _statistics.Searches++;
            TrySave();
        }

        public void IncreaseAdds()
        {
            _statistics.Adds++;
            TrySave();
        }

        public void IncreaseWebhooksReceived()
        {
            _statistics.WebhooksReceived++;
            TrySave();
        }

        public void IncreaseWebhooksProcessed()
        {
            _statistics.WebhooksProcessed++;
            TrySave();
        }
    }
}
