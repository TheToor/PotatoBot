using Newtonsoft.Json;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Plex;
using PotatoBot.Modals.API.Plex.Library;
using PotatoBot.Modals.API.Plex.Providers;
using PotatoBot.Modals.API.Plex.Release;
using PotatoBot.Modals.API.Plex.Statistics;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace PotatoBot.Services
{
    public class PlexService : IService
    {
        public string Name { get; }

        private string _plexSetupFile => $"plex.{Name.Replace(" ", "_")}.setup";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PlexSettings _plexSettings;
        private string _plexToken => _plexSettings.APIKey;

        private string _machineIdentifier;
        private MediaContainer _libraries;
        private readonly Dictionary<string, Video> _lastAddedMediaItem = new();
        private readonly Dictionary<string, Modals.API.Plex.Library.Directory> _lastAddedDirectory = new();

        internal PlexService(PlexSettings plexSettings)
        {
            Name = plexSettings.Name;

            _plexSettings = plexSettings ?? throw new ArgumentNullException(nameof(plexSettings));

            if(File.Exists(_plexSetupFile))
            {
                _logger.Trace("Setup initiated ...");

                var lines = File.ReadAllLines(_plexSetupFile);
                if(lines.Length < 2)
                {
                    throw new Exception($"Invalid {_plexSetupFile} file!");
                }

                var username = lines[0];
                var password = lines[1];

                _logger.Trace("Successfully read username and password. Generating new identifier ...");

                GetToken(username, password);
            }

            TestConnection();

            GetLibraries();

            _logger.Info($"Started {Name} Plex API");
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        private void TestConnection()
        {
            var response = GetXml<MediaContainer>(APIEndPoints.PlexEndpoints.Capabilities);
            if(response == null)
            {
                throw new Exception("TestConnection failed!");
            }

            _machineIdentifier = response.MachineIdentifier;

            _logger.Trace("======= System Info =======");
            _logger.Trace($"Name: {response.FriendlyName}");
            _logger.Trace($"Version: {response.Version}");
            _logger.Trace($"Platform: {response.Platform}");
            _logger.Trace($"PlatformVersion: {response.PlatformVersion}");

            var releaseStatus = GetRelease();
            if(releaseStatus.Release != null)
            {
                _logger.Trace($"New release available: {releaseStatus.Release.Key}");
            }
        }

        private List<Modals.API.Plex.Directory> GetLibraries()
        {
            var response = GetXml<MediaContainer>(APIEndPoints.PlexEndpoints.Library);

            _libraries = response ?? throw new Exception("Failed to get libraries");

            foreach(var library in response.Directory)
            {
                _logger.Trace($"[{library.Key}][{library.Type}] {library.Title}");
            }

            return response.Directory;
        }

        private List<Section> GetSections()
        {
            var response = GetXml<SectionMediaContainer>(
                string.Format(
                    APIEndPoints.PlexEndpoints.Section,
                    _machineIdentifier
                )
            );
            var sections = response?.Server?.Section ?? throw new Exception("Failed to get sections");
            return sections;
        }

        internal RecentlyAddedResult GetRecentlyAdded()
        {
            var newItems = new RecentlyAddedResult();
            foreach(var library in _libraries.Directory)
            {
                if(!Enum.TryParse<LibraryType>(library.Type, true, out var librarytype))
                {
                    _logger.Warn($"Failed to parse Library {library.Title} with type {library.Type}");
                    continue;
                }

                var allRecentlyAdded = GetRecentlyAdded(librarytype, uint.Parse(library.Key));

                if(allRecentlyAdded.Directory.Any())
                {
                    if(!_lastAddedDirectory.ContainsKey(library.Key))
                    {
                        // First time getting directories
                        newItems.NewDirectories = allRecentlyAdded.Directory;
                        _lastAddedDirectory.Add(library.Key, allRecentlyAdded.Directory.First());
                    }
                    else
                    {
                        // Get latest only
                        var lastDirectory = _lastAddedDirectory[library.Key];
                        foreach(var directory in allRecentlyAdded.Directory)
                        {
                            if(directory == lastDirectory)
                            {
                                break;
                            }
                            newItems.NewDirectories.Add(directory);
                        }
                    }
                }

                // Check for new episodes
                var recentlyAddedMedia = allRecentlyAdded.Video.OrderByDescending(v => v.AddedAtDate);
                if(!recentlyAddedMedia.Any())
                {
                    _logger.Trace($"Got no items for {library.Title}. Library empty?");
                    continue;
                }

                if(!_lastAddedMediaItem.ContainsKey(library.Key))
                {
                    // First time fetching
                    newItems.NewItems.AddRange(recentlyAddedMedia);
                    _lastAddedMediaItem.Add(library.Key, recentlyAddedMedia.First());
                    continue;
                }

                var lastChecked = _lastAddedMediaItem[library.Key];
                foreach(var item in recentlyAddedMedia)
                {
                    if(item == lastChecked)
                    {
                        break;
                    }
                    newItems.NewItems.Add(item);
                }
            }
            return newItems;
        }

        private static HttpClient GetHttpClient(bool json = false)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            if(!json)
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/xml")
                );
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
            }
            client.DefaultRequestHeaders.Add("User-Agent", Program.Namespace);

            return client;
        }

        private T GetXml<T>(string endpoint, bool hasParameters = false, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var url = new Uri($"{(endpoint.StartsWith("http") ? "" : $"{_plexSettings.Url}/")}{endpoint}{(hasParameters ? "&" : "?")}X-Plex-Token={_plexToken}");

            try
            {
                using var client = GetHttpClient();
                _logger.Trace($"Sending request to {url}");

                var response = client.GetAsync(url).Result;

                if(response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                }

                var xml = response.Content.ReadAsStringAsync().Result;
                if(string.IsNullOrEmpty(xml))
                {
                    _logger.Warn("Empty response!");
                    return default;
                }

                var serializer = new XmlSerializer(typeof(T));
                using var streamReader = new StringReader(xml);
                return (T)serializer.Deserialize(streamReader);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to get information from endpoint '{endpoint}'");
                return default;
            }
        }

        private T GetJson<T>(string endpoint, bool hasParameters = false, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var url = new Uri($"{_plexSettings.Url}/{endpoint}{(hasParameters ? "&" : "?")}X-Plex-Token={_plexToken}");

            try
            {
                using var client = GetHttpClient();
                _logger.Trace($"Sending request to {url}");

                var response = client.GetAsync(url).Result;

                if(response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                }

                var json = response.Content.ReadAsStringAsync().Result;
                if(string.IsNullOrEmpty(json))
                {
                    _logger.Warn("Empty response!");
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to get information from endpoint '{endpoint}'");
                return default;
            }
        }

        private void GetToken(string username, string password)
        {
            var guid = System.Guid.NewGuid().ToString();
            _logger.Trace($"Requesting new token with ID {guid}");

            using var client = GetHttpClient();
            var body = $"{username}:{password}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(body);
            var base64 = Convert.ToBase64String(bytes);

            client.DefaultRequestHeaders.Add("X-Plex-Client-Identifier", guid);
            client.DefaultRequestHeaders.Add("X-Plex-Product", Program.Namespace);
            client.DefaultRequestHeaders.Add("X-Plex-Version", Program.Version.ToString());
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64}");

            var response = client.PostAsync(APIEndPoints.PlexEndpoints.TokenGeneration, null).Result;

            if(response.StatusCode == HttpStatusCode.Created)
            {
                var text = response.Content.ReadAsStringAsync().Result;
                var token = JsonConvert.DeserializeObject<TokenResponse>(text);

                _plexSettings.APIKey = token.User.Authentication_token;
                Program.SaveSettings();

                File.Delete(_plexSetupFile);

                _logger.Info("Successfully generated Key for Plex. Plex API available");
            }
            else
            {
                _logger.Warn($"Invalid response recieved: {response.StatusCode}");
            }
        }

        internal bool Invite(string email)
        {
            var libraries = GetSections().Where(s => _plexSettings.LibrariesToShare.Contains(s.Title)).Select(s => s.Id).Cast<int>();

            // V2 API ?
            //var parameters = new Dictionary<string, dynamic>()
            //{
            //    { "machineIdentifier", _machineIdentifier },
            //    {
            //        "librarySectionIds",
            //        libraries
            //    },
            //    {
            //        "settings",
            //        new Dictionary<string, string>()
            //        {
            //            { "allowSync", "1" },
            //            { "allowCameraUpload", "0" },
            //            { "filterMovies", "" },
            //            { "filterTelevision", "" },
            //            { "filterMusic", "" }
            //        }
            //    },
            //    { "invitedEmail", email }
            //};

            var parameters = new Dictionary<string, dynamic>()
            {
                { "server_id", _machineIdentifier },
                { "shared_server", new Dictionary<string, dynamic>()
                    {
                        { "library_section_ids", libraries },
                        { "invited_email", email }
                    }
                },
                { "sharing_settings", new Dictionary<string, string>()
                    {
                        { "allowSync", "1" },
                        { "allowCameraUpload", "0" },
                        { "filterMovies", "" },
                        { "filterTelevision", "" },
                        { "filterMusic", "" }
                    }
                }
            };

            var body = JsonConvert.SerializeObject(parameters);
            var client = GetHttpClient(true);
            var response = client.PostAsync(
                string.Format(
                    APIEndPoints.PlexEndpoints.Invite,
                    _machineIdentifier,
                    $"?X-Plex-Token={_plexToken}"
                ),
                new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            ).Result;
            if(response.IsSuccessStatusCode)
            {
                // V2 API
                //var responseBody = response.Content.ReadAsStringAsync().Result;
                //var plexResponse = JsonConvert.DeserializeObject<InviteResponse>(responseBody);
                return true;
            }
            return false;
        }

        internal StatisticsMediaContainer GetMediaStatistics()
        {
            var response = GetXml<StatisticsMediaContainer>(APIEndPoints.PlexEndpoints.MediaStatistics, true);
            return response ?? new ();
        }

        internal ReleaseContainer GetRelease()
        {
            var response = GetXml<ReleaseContainer>(APIEndPoints.PlexEndpoints.UpdateStatus);
            return response ?? new ();
        }

        internal ProviderMediaContainer GetMediaProviders()
        {
            var response = GetXml<ProviderMediaContainer>(APIEndPoints.PlexEndpoints.MediaProviders);
            return response ?? new();
        }

        private LibraryMediaContainer GetRecentlyAdded(LibraryType libraryType, uint libraryId)
        {
            var response = GetXml<LibraryMediaContainer>(
                string.Format(
                    APIEndPoints.PlexEndpoints.RecentlyAdded, // Endpoint
                    (int)libraryType, // Type (movie, show, artist)
                    libraryId, // LibraryId
                    24 // How many items
                ),
                true
            );
            return response ?? new();
        }
    }
}
