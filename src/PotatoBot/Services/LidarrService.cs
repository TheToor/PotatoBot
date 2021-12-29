using PotatoBot.API;
using PotatoBot.Modals;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.Requests.POST;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;

namespace PotatoBot.Services
{
	public class LidarrService : APIBase, IService, IServarr
	{
		public ServarrType Type => ServarrType.Lidarr;

		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly object _albumLock = new();
		// AristId -> AlbumId -> Album
		private readonly Dictionary<ulong, Dictionary<ulong, Album>> _albumCache = new();

		internal LidarrService(EntertainmentSettings settings, string apiUrl) : base(settings, apiUrl)
		{
		}

		public bool Start()
		{
			return true;
		}

		public bool Stop()
		{
			return true;
		}

		public IEnumerable<IServarrItem> GetAll()
		{
			_logger.Trace("Fetching all artists  ...");

			var response = GetRequest<List<Artist>>(APIEndPoints.LidarrEndpoints.Artist);
			_logger.Trace($"Got {response.Count} artists as a response");

			return response;
		}

		public IEnumerable<IServarrItem> Search(string name)
		{
			_logger.Trace($"Searching artist with name {name} ...");

			var requestBody = new LookupRequest()
			{
				SearchTerm = name
			};

			var response = GetRequest<List<Artist>>(APIEndPoints.LidarrEndpoints.Lookup, requestBody);
			_logger.Trace($"Got {response.Count} artists as response");

			return response;
		}

		public AddResult Add(IServarrItem item)
		{
			var artist = item as Artist ?? throw new ArgumentNullException(nameof(item));

			_logger.Trace($"Adding artist [{artist.Id}] {artist.ArtistName}");

			var postBody = new AddArtist(this, artist);

			var response = PostRequest<Artist>(APIEndPoints.LidarrEndpoints.Artist, postBody, System.Net.HttpStatusCode.Created);
			if(response.Item1 != null)
			{
				var aristResponse = response.Item1;
				_logger.Trace($"Successfully added {aristResponse.ArtistName} ({aristResponse.Path})");
			}
			return new AddResult
			{
				Added = response.Item1 != null,
				AlreadyAdded = response.Item2 == System.Net.HttpStatusCode.BadRequest,
				StatusCode = response.Item2
			};
		}

		public override List<QueueItem> GetQueue()
		{
			_logger.Trace("Fetching download queue");

			var response = GetRequest<Modals.API.Queue<LidarrQueueItem>>(APIEndPoints.Queue, new QueueRequest());
			if(response != null)
			{
				_logger.Trace("Successfully fetched download queue");
				var list = new List<QueueItem>();
				foreach(var record in response.Records)
				{
					record.API = this;
					list.Add(record);
				}
				return list;
			}
			return null;
		}

		internal Album GetAlbumInfo(ulong artistId, ulong albumId)
		{
			_logger.Trace($"Getting album info for artist {artistId} and album {albumId}");

			lock(_albumLock)
			{
				if(_albumCache.ContainsKey(artistId) && _albumCache[artistId].ContainsKey(albumId))
				{
					_logger.Trace($"Fetched album from cache");
					return _albumCache[artistId][albumId];
				}

				var endpint = string.Format(APIEndPoints.LidarrEndpoints.Album, albumId);
				var requestBody = new RequestAlbum()
				{
					ArtistId = artistId
				};

				var response = GetRequest<Album>(endpint, requestBody);
				if(response != null)
				{
					if(!_albumCache.ContainsKey(artistId))
					{
						_albumCache.Add(artistId, new Dictionary<ulong, Album>());
					}
					_albumCache[artistId].Add(albumId, response);

					_logger.Trace($"Successfully fetched album info");
				}
				return response;
			}
		}
	}
}
