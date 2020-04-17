using PotatoBot.Services;
using System;
using System.Collections.Generic;

namespace PotatoBot.Managers
{
    internal class ServiceManager
    {
        private SonarrService _sonarr;
        internal SonarrService Sonarr
        {
            get
            {
                if(!Program.Settings.Sonarr.Enabled)
                {
                    throw new Exception("Sonarr Service is not enabled");
                }
                return _sonarr;
            }
        }

        internal TelegramService TelegramService { get; } = new TelegramService();

        private List<IService> _services = new List<IService>()
        {
            new WebhookService()
        };

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal ServiceManager()
        {
            _logger.Info("ServiceManager starting ...");

            _services.Add(TelegramService);

            var settings = Program.Settings;
            if(settings.Sonarr.Enabled)
            {
                _logger.Info("Enabling Sonarr Service ...");
                _sonarr = new SonarrService(settings.Sonarr, "api/v3");
                _services.Add(_sonarr);
            }

            StartAllServices();
        }

        ~ServiceManager()
        {
            StopAllServices();
        }

        private void StartAllServices()
        {
            _logger.Info($"Starting {_services.Count} services ...");
            foreach(var service in _services)
            {
                _logger.Trace($"Trying to start {service.Name} ...");

                if(service.Start())
                {
                    _logger.Info($"Successfully started {service.Name}");
                }
                else
                {
                    _logger.Warn($"Failed to start {service.Name}");
                }
            }
        }

        private void StopAllServices()
        {
            _logger.Info($"Stopping {_services.Count} services ...");
            foreach (var service in _services)
            {
                _logger.Trace($"Trying to stop {service.Name} ...");

                if (service.Stop())
                {
                    _logger.Info($"Successfully stopped {service.Name}");
                }
                else
                {
                    _logger.Warn($"Failed to stop {service.Name}");
                }
            }
        }
    }
}
