using CacheManager.Core;
using Newtonsoft.Json;
using PotatoBot.Modals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PotatoBot.Services
{
    public class WatchListService : IService
    {
        public string Name => "WatchList";

        private const string WatchListDatabaseFileName = "watchlist.json";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private System.Timers.Timer _timer;

        private readonly DateTime _settingsLastSaved = DateTime.MinValue;
        /// <summary>
        /// Long = Telegram User Id
        /// String = IServarr Service Name
        /// List<int> Watched items in IServarr Service
        /// </summary>
        private Dictionary<long, Dictionary<string, List<ulong>>> _watchList = new();
        private readonly object _watchListLock = new ();
        private ICacheManager<object> _watchListPathCache;

        public bool Start()
        {
            _timer = new System.Timers.Timer(5 * 60 * 1000);
            _timer.Elapsed += CheckWatchlist;
            _timer.AutoReset = true;
            _timer.Start();

            _watchListPathCache = CacheFactory.Build("WatchList", settings =>
            {
                settings
                    .WithSystemRuntimeCacheHandle()
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(60));
            });

            // Delay reading of settings
            Task.Factory.StartNew(async () =>
            {
                do
                {
                    await Task.Delay(1000);
                }
                while(Program.ServiceManager == null);

                ReadSettings();
            });

            return true;
        }

        public bool Stop()
        {
            SaveSettings(true);

            _watchListPathCache.Dispose();

            _timer.Stop();
            _timer.Dispose();
            return true;
        }

        private void ReadSettings()
        {
            lock(_watchListLock)
            {
                _logger.Trace("Reading watchlist ...");

                if(!File.Exists(WatchListDatabaseFileName))
                {
                    File.WriteAllText(WatchListDatabaseFileName, JsonConvert.SerializeObject(_watchList));
                }
                _watchList = JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, List<ulong>>>>(File.ReadAllText(WatchListDatabaseFileName));
            }
        }

        private void SaveSettings(bool force = false)
        {
            // Do not spam save
            if(!force && _settingsLastSaved.AddMinutes(5) > DateTime.Now)
            {
                return;
            }

            lock(_watchListLock)
            {
                _logger.Trace("Saving watchlist");
                if(File.Exists(WatchListDatabaseFileName))
                {
                    File.Delete(WatchListDatabaseFileName);
                }
                File.WriteAllText(WatchListDatabaseFileName, JsonConvert.SerializeObject(_watchList));
            }
        }

        internal void CheckWatchlist(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock(_watchListLock)
            {
                var plexServices = Program.ServiceManager.GetPlexServices().ToDictionary(p => p, p => p.GetRecentlyAdded());

                var serviceCache = new Dictionary<string, IServarr>();

                var newWatchList = new Dictionary<long, Dictionary<string, List<ulong>>>();

                foreach(var user in _watchList.Keys)
                {
                    newWatchList.Add(user, new ());

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

                                if(Program.ServiceManager.GetAllServices().FirstOrDefault(s => s is IServarr && s.Name == service) is not IServarr selectedService)
                                {
                                    _logger.Warn($"Invalid item in cache from {user}. Service {service} is invalid");
                                    return null;
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

                            _logger.Trace($"Transforming path. Current path is '{path}'");
                            foreach(var plex in plexServices.Keys)
                            {
                                var settings = Program.Settings.Plex.FirstOrDefault(s => s.Name == plex.Name);
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
                                    Program.ServiceManager.TelegramService.SendSimpleMessage(
                                        user,
                                        string.Format(
                                            Program.LanguageManager.GetTranslation("Commands", "Plex", "Added"),
                                            releaseItem.Title
                                        ),
                                        Telegram.Bot.Types.Enums.ParseMode.Html
                                    ).Wait();
                                    continue;
                                }

                                var releaseDirectory = newItems.NewDirectories.FirstOrDefault(d => 
                                    (d.Location?.Path?.StartsWith(path) ?? false) ||
                                    (d.Location?.Path == path)
                                );
                                if(releaseDirectory != null)
                                {
                                    Program.ServiceManager.TelegramService.SendSimpleMessage(
                                        user,
                                        string.Format(
                                            Program.LanguageManager.GetTranslation("Commands", "Plex", "Added"),
                                            releaseDirectory.Title
                                        ),
                                        Telegram.Bot.Types.Enums.ParseMode.Html
                                    ).Wait();
                                    continue;
                                }

                                // Readd to watchlist
                                newWatchList[user][service].Add(item);
                            }
                        }
                    }
                }

                // Update watchlist
                _watchList = newWatchList;
            }
        }

        internal void AddToWatchList(long userId, IServarr service, IServarrItem item)
        {
            lock(_watchListLock)
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

            SaveSettings();
        }
    }
}
