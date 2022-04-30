using PotatoBot.Model.Settings;
using System;
using System.Collections.Generic;

namespace PotatoBot.Services
{
    public class ServiceManager : IDisposable
    {
        private readonly List<SonarrService> _sonarr = new();
        internal List<SonarrService> Sonarr => _sonarr;

        private readonly List<RadarrService> _radarr = new();
        internal List<RadarrService> Radarr => _radarr;

        private readonly List<LidarrService> _lidarr = new();
        internal List<LidarrService> Lidarr => _lidarr;

        private readonly List<IService> _services = new();

        private readonly List<SABnzbdService> _sabNzbdServices = new();
        private readonly List<PlexService> _plexServices = new();

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public ServiceManager(BotSettings settings)
        {
            _logger.Info("ServiceManager starting ...");

            InitializeSonarr(settings);

            InitializeRadarr(settings);

            InitializeLidarr(settings);

            InitializePlex(settings);

            InitializeSABnzbd(settings);

            StartAllServices();
        }
        public void Dispose()
        {
            StopAllServices();
            GC.SuppressFinalize(this);
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

        private void InitializeSABnzbd(BotSettings settings)
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

        private void InitializePlex(BotSettings settings)
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

                    var service = new PlexService(settings, server);
                    _plexServices.Add(service);
                    _services.Add(service);
                }
            }
        }

        private void InitializeLidarr(BotSettings settings)
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
                        Controllers.Calendar.Calendars.Add(lidarrService);
                    }
                }
            }
        }

        private void InitializeRadarr(BotSettings settings)
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
                        Controllers.Calendar.Calendars.Add(radarrService);
                    }
                }
            }
        }

        private void InitializeSonarr(BotSettings settings)
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
                        Controllers.Calendar.Calendars.Add(sonarrService);
                    }
                }
            }
        }
    }
}
