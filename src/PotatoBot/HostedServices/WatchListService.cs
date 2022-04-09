using CacheManager.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PotatoBot.Managers;
using PotatoBot.Modals;
using PotatoBot.Modals.Settings;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;

namespace PotatoBot.HostedServices
{
    public class WatchListService : IHostedService
    {
        public string Name => "WatchList";

        private const string WatchListDatabaseFileName = "watchlist.json";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly System.Timers.Timer _timer;

        private readonly DateTime _settingsLastSaved = DateTime.MinValue;
        /// <summary>
        /// Long = Telegram User Id
        /// String = IServarr Service Name
        /// List<int> Watched items in IServarr Service
        /// </summary>
        private Dictionary<long, Dictionary<string, List<ulong>>> _watchList = new();
        private readonly SemaphoreSlim _watchListLock = new(1);
        private readonly ICacheManager<object> _watchListPathCache;

        private readonly ServiceManager _serviceManager;
        private readonly TelegramService _telegramService;
        private readonly LanguageManager _languageManager;
        private readonly BotSettings _botSettings;

        public WatchListService(ServiceManager serviceManager, TelegramService telegramService, LanguageManager languageManager, BotSettings botSettings)
        {
            _serviceManager = serviceManager;
            _telegramService = telegramService;
            _languageManager = languageManager;
            _botSettings = botSettings;

            _timer = new System.Timers.Timer(5 * 60 * 1000);
            _timer.Elapsed += CheckWatchlist;
            _timer.AutoReset = true;

            _watchListPathCache = CacheFactory.Build("WatchList", settings =>
            {
                settings
                    .WithSystemRuntimeCacheHandle()
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(60));
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Start();
            ReadSettings();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            SaveSettings(true);

            _watchListPathCache.Dispose();

            _timer.Stop();
            _timer.Dispose();
        }

        private void ReadSettings()
        {
            _logger.Trace("Waiting for lock");
            _watchListLock.Wait();
            try
            {
                _logger.Trace("Reading watchlist ...");

                if(!File.Exists(WatchListDatabaseFileName))
                {
                    File.WriteAllText(WatchListDatabaseFileName, JsonConvert.SerializeObject(_watchList));
                }
                _watchList = JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, List<ulong>>>>(File.ReadAllText(WatchListDatabaseFileName));
            }
            finally
            {
                _watchListLock.Release();
            }
        }

        private void SaveSettings(bool force = false)
        {
            // Do not spam save
            if(!force && _settingsLastSaved.AddMinutes(5) > DateTime.Now)
            {
                return;
            }

            _watchListLock.Wait();
            try
            {
                _logger.Trace("Saving watchlist");
                if(File.Exists(WatchListDatabaseFileName))
                {
                    File.Delete(WatchListDatabaseFileName);
                }
                File.WriteAllText(WatchListDatabaseFileName, JsonConvert.SerializeObject(_watchList));
            }
            finally
            {
                _watchListLock.Release();
            }
        }

        internal void CheckWatchlist(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckWatchlistAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        internal async Task CheckWatchlistAsync()
        {
            await _watchListLock.WaitAsync();

            try
            {
                var plexServices = _serviceManager.GetPlexServices().ToDictionary(p => p, p => p.GetRecentlyAdded());

                var serviceCache = new Dictionary<string, IServarr>();

                var newWatchList = new Dictionary<long, Dictionary<string, List<ulong>>>();

                foreach(var user in _watchList.Keys)
                {
                    newWatchList.Add(user, new());

                    var services = _watchList[user];
                    foreach(var service in services.Keys)
                    {
                        newWatchList[user].Add(service, new());

                        var items = services[service];
                        foreach(var item in items)
                        {
                            var itemKey = $"{service}_{item}";
                            var path = (string)_watchListPathCache.GetOrAdd(itemKey, (itemKey) =>
                            {
                                var itemId = ulong.Parse(itemKey.Split('_')[1]);
                                if(serviceCache.ContainsKey(service))
                                {
                                    return serviceCache[service].GetById(itemId).Path;
                                }

                                if(_serviceManager.GetAllServices().FirstOrDefault(s => s is IServarr && s.Name == service) is not IServarr selectedService)
                                {
                                    throw new Exception($"Invalid item in cache from {user}. Service {service} is invalid");
                                }
                                serviceCache.Add(service, selectedService);
                                return selectedService.GetById(itemId).Path;
                            });

                            if(string.IsNullOrEmpty(path))
                            {
                                // If we cant find the item there is also no reason to readd it to the watchlist
                                // So we just "drop" it
                                _logger.Trace($"Path for {itemKey} is empty. Ignoring");
                                continue;
                            }

                            try
                            {
                                _logger.Trace($"Transforming path. Current path is '{path}'");
                                foreach(var plex in plexServices.Keys)
                                {
                                    var settings = _botSettings.Plex.FirstOrDefault(s => s.Name == plex.Name);
                                    if(settings != null)
                                    {
                                        foreach(var pathTransform in settings.PathOverrides)
                                        {
                                            path = path.Replace(pathTransform.Key, pathTransform.Value);
                                        }
                                    }

                                    var newItems = plexServices[plex];
                                    _logger.Trace($"Searching for '{path}' in {newItems.NewItems.Count} items and {newItems.NewDirectories.Count} new releases");
                                    var releaseItem = newItems.NewItems.FirstOrDefault(i => i.Media.Part.File.StartsWith(path));
                                    if(releaseItem != null)
                                    {
                                        await _telegramService.SendSimpleAlertMessage(
                                            user,
                                            string.Format(
                                                _languageManager.GetTranslation("Commands", "Plex", "Added"),
                                                releaseItem.Title
                                            ),
                                            Telegram.Bot.Types.Enums.ParseMode.Html
                                        );
                                        continue;
                                    }

                                    var releaseDirectory = newItems.NewDirectories.FirstOrDefault(d =>
                                        (d.Location?.Path?.StartsWith(path) ?? false) ||
                                        d.Location?.Path == path
                                    );
                                    if(releaseDirectory != null)
                                    {
                                        await _telegramService.SendSimpleAlertMessage(
                                            user,
                                            string.Format(
                                                _languageManager.GetTranslation("Commands", "Plex", "Added"),
                                                releaseDirectory.Title
                                            ),
                                            Telegram.Bot.Types.Enums.ParseMode.Html
                                        );
                                        continue;
                                    }

                                    // Readd to watchlist
                                    newWatchList[user][service].Add(item);
                                }
                            }
                            catch(ApiRequestException ex)
                            {
                                _logger.Warn(ex);
                                if(newWatchList[user][service].Contains(item))
                                {
                                    newWatchList[user][service].Remove(item);
                                    _logger.Debug($"Removed faulty item from {user} watchlist");
                                }
                            }
                        }
                    }
                }

                // Update watchlist
                _watchList = newWatchList;
            }
            finally
            {
                _watchListLock.Release();
            }
        }

        internal void AddToWatchList(long userId, IServarr service, IServarrItem item)
        {
            _watchListLock.Wait();
            try
            {
                if(!_watchList.ContainsKey(userId))
                {
                    _watchList.Add(userId, new());
                }
                if(!_watchList[userId].ContainsKey(service.Name))
                {
                    _watchList[userId].Add(service.Name, new());
                }
                if(_watchList[userId][service.Name] == null)
                {
                    _watchList[userId][service.Name] = new();
                }
                if(_watchList[userId][service.Name].Contains(item.Id))
                {
                    // Already on watch list
                    return;
                }
                _watchList[userId][service.Name].Add(item.Id);

                _logger.Trace($"Added {item.Id} in service {service.Name} to watchlist for user {userId}");
            }
            finally
            {
                _watchListLock.Release();
            }

            SaveSettings();
        }
    }
}
