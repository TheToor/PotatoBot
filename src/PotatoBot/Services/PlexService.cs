using Newtonsoft.Json;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Plex;
using PotatoBot.Modals.API.Plex.Statistics;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace PotatoBot.Services
{
	internal class PlexService : IService
	{
		public string Name { get; }

		private string _plexSetupFile => $"plex.{Name.Replace(" ", "_")}.setup";

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly PlexSettings _plexSettings;
		private string _plexToken => _plexSettings.APIKey;


		private MediaContainer _libraries;

		private readonly Dictionary<int, DateTime> _lastAPIRescanInitiated = new();

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

			_logger.Trace("======= System Info =======");
			_logger.Trace($"Name: {response.FriendlyName}");
			_logger.Trace($"Version: {response.Version}");
			_logger.Trace($"Platform: {response.Platform}");
			_logger.Trace($"PlatformVersion: {response.PlatformVersion}");
		}

		private void GetLibraries()
		{
			var response = GetXml<MediaContainer>(APIEndPoints.PlexEndpoints.Library);

			_libraries = response ?? throw new Exception("Failed to get libraries");

			foreach(var library in response.Directory)
			{
				_logger.Trace($"[{library.Key}][{library.Type}] {library.Title}");
			}
		}

		private static HttpClient GetHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("text/xml")
			);
			client.DefaultRequestHeaders.Add("User-Agent", Program.Namespace);

			return client;
		}

		private T GetXml<T>(string endpoint, bool hasParameters = false, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
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

		private void Get(string endpoint, bool hasParameters = false, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
		{
			var url = $"{_plexSettings.Url}/{endpoint}{(hasParameters ? "" : "?")}X-Plex-Token={_plexToken}";

			try
			{
				using var client = GetHttpClient();
				_logger.Trace($"Sending request to {url}");

				var response = client.GetAsync(url).Result;

				if(response.StatusCode != expectedStatusCode)
				{
					_logger.Warn($"Unexpected Status Code: {response.StatusCode}");
				}
			}
			catch(Exception ex)
			{
				_logger.Error(ex, $"Failed to get information from endpoint '{endpoint}'");
			}
		}

		private void GetToken(string username, string password)
		{
			var guid = Guid.NewGuid().ToString();
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

		internal StatisticsMediaContainer GetMediaStatistics()
		{
			var response = GetXml<StatisticsMediaContainer>(APIEndPoints.PlexEndpoints.MediaStatistics, true);
			return response ?? new StatisticsMediaContainer();
		}

		internal void RescanMediaLibraries(int[] libraries)
		{
			_logger.Trace("Rescanning libraries ...");

			foreach(var library in libraries)
			{
				try
				{
					if(_lastAPIRescanInitiated.ContainsKey(library))
					{
						var lastRescan = _lastAPIRescanInitiated[library];
						if(lastRescan.AddMinutes(1) > DateTime.Now)
						{
							_logger.Trace($"Skipping {library} as last rescan was less than a minute ago");
							continue;
						}

						_lastAPIRescanInitiated.Remove(library);
					}
					_logger.Trace($"Sending request to rescan {library} ...");
					Get(
						string.Format(
							APIEndPoints.PlexEndpoints.LibraryUpdate,
							library
						)
					);

					_lastAPIRescanInitiated.Add(library, DateTime.Now);
				}
				catch(Exception ex)
				{
					_logger.Warn(ex, $"Failed to initiate rescan for {library}");
				}
			}
		}
	}
}
