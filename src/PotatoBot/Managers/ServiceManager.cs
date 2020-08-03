using NodaTime;
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

        private RadarrService _radarr;
        internal RadarrService Radarr
        {
            get
            {
                if(!Program.Settings.Radarr.Enabled)
                {
                    throw new Exception("Radarr Service is not enabled");
                }
                return _radarr;
            }
        }

        private LidarrService _lidarr;
        internal LidarrService Lidarr
        {
            get
            {
                if(!Program.Settings.Lidarr.Enabled)
                {
                    throw new Exception("Lidarr Service is not enabled");
                }
                return _lidarr;
            }
        }

        private PlexService _plex;
        internal PlexService Plex
        {
            get
            {
                if(!Program.Settings.Plex.Enabled)
                {
                    throw new Exception("Plex Service is not enabled");
                }
                return _plex;
            }
        }

        internal TelegramService TelegramService { get; } = new TelegramService();
        internal StatisticsService StatisticsService { get; } = new StatisticsService();

        private List<IService> _services = new List<IService>()
        {
            new WebhookService()
        };

        private List<SABnzbdService> _sabNzbdServices = new List<SABnzbdService>();

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal ServiceManager()
        {
            _logger.Info("ServiceManager starting ...");

            _services.Add(TelegramService);
            _services.Add(StatisticsService);

            var settings = Program.Settings;
            
            if(settings.Sonarr.Enabled)
            {
                _logger.Info("Enabling Sonarr Service ...");
                _sonarr = new SonarrService(settings.Sonarr, "api/v3");
                _services.Add(_sonarr);

                API.Calendar.Calendars.Add(_sonarr);
            }

            if(settings.Radarr.Enabled)
            {
                _logger.Info("Enabling Radarr Service ...");
                _radarr = new RadarrService(settings.Radarr, "api");
                _services.Add(_radarr);

                API.Calendar.Calendars.Add(_radarr);
            }

            if(settings.Lidarr.Enabled)
            {
                _logger.Info("Enalbing Lidarr Service ...");
                _lidarr = new LidarrService(settings.Lidarr, "api/v1");
                _services.Add(_lidarr);

                API.Calendar.Calendars.Add(_lidarr);
            }

            if(settings.Plex.Enabled)
            {
                _logger.Info("Enalbing Plex Service ...");
                _plex = new PlexService();
                _services.Add(_plex);
            }
            else
            {
                _logger.Info("Plex not enabled. Disabling RescanAfterDownload feature");

                Program.Settings.Radarr.RescanAfterDownload = false;
                Program.Settings.Sonarr.RescanAfterDownload = false;
                Program.Settings.Lidarr.RescanAfterDownload = false;
            }

            if(settings.SABnzbd?.Count > 0)
            {
                _logger.Info($"Adding {settings.SABnzbd.Count} SABnzbd Servers");

                foreach(var server in settings.SABnzbd)
                {
                    if(!server.Enabled)
                    {
                        _logger.Trace($"Skipping '{server.Url}' because it is disabled");
                        continue;
                    }

                    var service = new SABnzbdService(server);
                    _sabNzbdServices.Add(service);
                    _services.Add(service);
                }
            }

            StartAllServices();
        }

        ~ServiceManager()
        {
            StopAllServices();
        }

        internal void StartAllServices()
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

        internal void StopAllServices()
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

        internal List<SABnzbdService> GetSABnzbdServices()
        {
            return _sabNzbdServices;
        }
    }
}
