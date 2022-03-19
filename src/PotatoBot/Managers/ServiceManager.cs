using PotatoBot.Services;
using System.Collections.Generic;

namespace PotatoBot.Managers
{
    internal class ServiceManager
    {
        private readonly List<SonarrService> _sonarr = new();
        internal List<SonarrService> Sonarr => _sonarr;

        private readonly List<RadarrService> _radarr = new();
        internal List<RadarrService> Radarr => _radarr;

        private readonly List<LidarrService> _lidarr = new();
        internal List<LidarrService> Lidarr => _lidarr;

        internal TelegramService TelegramService { get; } = new TelegramService();
        internal StatisticsService StatisticsService { get; } = new StatisticsService();
        internal WatchListService WatchListService { get; } = new WatchListService();

        private readonly List<IService> _services = new();

        private readonly List<SABnzbdService> _sabNzbdServices = new();
        private readonly List<PlexService> _plexServices = new();

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal ServiceManager()
        {
            _logger.Info("ServiceManager starting ...");

            // TelegramService relies on StatisticsService
            _services.Add(StatisticsService);
            _services.Add(TelegramService);
            _services.Add(WatchListService);

            var settings = Program.Settings;

            InitializeSonarr(settings);

            InitializeRadarr(settings);

            InitializeLidarr(settings);

            InitializePlex(settings);

            InitializeSABnzbd(settings);

            // Webhook should be started as the last service as it may depend on other services (like Sonarr)
            _services.Add(new WebhookService());

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
            foreach(var service in _services)
            {
                _logger.Trace($"Trying to stop {service.Name} ...");

                if(service.Stop())
                {
                    _logger.Info($"Successfully stopped {service.Name}");
                }
                else
                {
                    _logger.Warn($"Failed to stop {service.Name}");
                }
            }
        }

        internal List<IService> GetAllServices()
        {
            return _services;
        }

        internal List<SABnzbdService> GetSABnzbdServices()
        {
            return _sabNzbdServices;
        }

        internal List<PlexService> GetPlexServices()
        {
            return _plexServices;
        }

        private void InitializeSABnzbd(Modals.Settings.BotSettings settings)
        {
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
        }

        private void InitializePlex(Modals.Settings.BotSettings settings)
        {
            if(settings.Plex?.Count > 0)
            {
                _logger.Info($"Adding {settings.Plex.Count} Plex Servers");

                foreach(var server in settings.Plex)
                {
                    if(!server.Enabled)
                    {
                        _logger.Trace($"Skipping '{server.Url}' because it is disabled");
                        continue;
                    }

                    var service = new PlexService(server);
                    _plexServices.Add(service);
                    _services.Add(service);
                }
            }
        }

        private void InitializeLidarr(Modals.Settings.BotSettings settings)
        {
            foreach(var lidarr in settings.Lidarr)
            {
                if(lidarr.Enabled)
                {
                    _logger.Info("Enalbing Lidarr Service ...");
                    var lidarrService = new LidarrService(lidarr, "api/v1");
                    _services.Add(lidarrService);
                    _lidarr.Add(lidarrService);

                    if(lidarr.EnableCalendar)
                    {
                        API.Calendar.Calendars.Add(lidarrService);
                    }
                }
            }
        }

        private void InitializeRadarr(Modals.Settings.BotSettings settings)
        {
            foreach(var radarr in settings.Radarr)
            {
                if(radarr.Enabled)
                {
                    _logger.Info("Enabling Radarr Service ...");
                    var radarrService = new RadarrService(radarr, "api/v3");
                    _services.Add(radarrService);
                    _radarr.Add(radarrService);

                    if(radarr.EnableCalendar)
                    {
                        API.Calendar.Calendars.Add(radarrService);
                    }
                }
            }
        }

        private void InitializeSonarr(Modals.Settings.BotSettings settings)
        {
            foreach(var sonarr in settings.Sonarr)
            {
                if(sonarr.Enabled)
                {
                    _logger.Info("Enabling Sonarr Service ...");
                    var sonarrService = new SonarrService(sonarr, "api/v3");
                    _services.Add(sonarrService);
                    _sonarr.Add(sonarrService);

                    if(sonarr.EnableCalendar)
                    {
                        API.Calendar.Calendars.Add(sonarrService);
                    }
                }
            }
        }
    }
}
