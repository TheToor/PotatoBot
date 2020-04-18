using PotatoBot.API;
using PotatoBot.Modals;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.Requests.POST;
using PotatoBot.Modals.Settings;
using System.Collections.Generic;

namespace PotatoBot.Services
{
    internal class RadarrService : APIBase, IService
    {
        public string Name => "Radarr";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal RadarrService(EntertainmentSettings settings, string apiUrl) : base(settings, apiUrl)
        {
            var systemStatus = GetSystemStatus();
            _logger.Trace("======= System Info =======");
            _logger.Trace($"Version: {systemStatus.Version}");
            _logger.Trace($"OS: {systemStatus.OSVersion}");
            _logger.Trace($"IsMono: {systemStatus.IsMono}");
            _logger.Trace($"IsLinux: {systemStatus.IsLinux}");
            _logger.Trace($"IsWindows: {systemStatus.IsWindows}");
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        internal List<RadarrQueueItem> GetQueue()
        {
            _logger.Trace("Fetching download queue");

            var response = GetRequest<List<RadarrQueueItem>>(APIEndPoints.Queue);
            if (response != null)
            {
                _logger.Trace("Successfully fetched download queue");
            }
            return response;
        }

        public List<Movie> SearchMovieByName(string name)
        {
            _logger.Trace($"Searching movie with name {name} ...");
            var requestBody = new LookupRequest()
            {
                SearchTerm = name
            };

            var response = GetRequest<List<Movie>>(APIEndPoints.Radarr.Lookup, requestBody);
            _logger.Trace($"Got {response.Count} movies as response");

            return response;
        }

        public AddResult AddMovie(Movie movie)
        {
            _logger.Trace($"Adding movie [{movie.TMDBId}] {movie.Title}");

            var postBody = new AddMovie(movie);

            var response = PostRequest<Movie>(APIEndPoints.Radarr.Movie, postBody, System.Net.HttpStatusCode.Created);
            if (response.Item1 != null)
            {
                var movieResponse = response.Item1;
                _logger.Trace($"Successfully added {movieResponse.Title} ({movieResponse.Path})");
            }
            return new AddResult
            {
                Added = response.Item1 != null,
                AlreadyAdded = response.Item2 == System.Net.HttpStatusCode.BadRequest,
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}
