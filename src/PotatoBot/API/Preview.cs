using Microsoft.AspNetCore.Mvc;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Sonarr;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PotatoBot.API
{
    [Route("[controller]", Name = "Preview")]
    public class Preview : Controller
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static uint _cacheDuration = 30;
        private static DateTime _nextCacheUpdate = DateTime.MinValue;

        private static List<BasicMovie> _movieCache;
        private static List<BasicSeries> _seriesCache;
        private static List<BasicArtist> _artistCache;

        private static Dictionary<string, object> _cachedResponse;

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            UpdateCacheIfRequired();

            if(_cachedResponse == null)
            {
                _cachedResponse = new Dictionary<string, object>();

                if(Program.Settings.Radarr.Enabled)
                {
                    _cachedResponse.Add("Movies", _movieCache);
                }
                if(Program.Settings.Sonarr.Enabled)
                {
                    _cachedResponse.Add("Series", _seriesCache);
                }
                if(Program.Settings.Lidarr.Enabled)
                {
                    _cachedResponse.Add("Artists", _artistCache);
                }
            }

            return Json(_cachedResponse);
        }

        [Route("poster/{type}")]
        [HttpGet]
        public async Task<IActionResult> GetPoster(string type, [FromQuery(Name = "url")] string requestUrl)
        {
            var libraryUrl = string.Empty;
            switch(type)
            {
                case "movie":
                    libraryUrl = Program.Settings.Radarr.Url;
                    break;

                case "series":
                    libraryUrl = Program.Settings.Sonarr.Url;
                    break;

                case "artist":
                    libraryUrl = Program.Settings.Lidarr.Url;
                    break;
            }

            var imageBytes = await DownloadPoster(libraryUrl, requestUrl);

            return File(imageBytes, "image/jpeg");
        }

        private static async Task<byte[]> DownloadPoster(string libraryUrl, string requestUrl)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", Program.Namespace);

            var url = $"{libraryUrl}{requestUrl}";

            _logger.Trace($"Fetching poster from '{url}'");
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                _logger.Warn($"Failed to fetch poster from '{url}'");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"Failed to fetch poster from '{url}'");
                return Array.Empty<byte>();
            }
        }

        private static void UpdateCacheIfRequired()
        {
            if(DateTime.Now < _nextCacheUpdate)
            {
                _logger.Trace("No cache update required");
                return;
            }

            _movieCache?.Clear();
            _movieCache = null;
            _seriesCache?.Clear();
            _seriesCache = null;
            _artistCache?.Clear();
            _artistCache = null;

            if (Program.Settings.Radarr.Enabled)
            {
                var movies = Program.ServiceManager.Radarr.GetAllMovies();
                if (movies != null)
                {
                    _movieCache = movies.ConvertAll((o) => new BasicMovie(o));
                }
            }

            if (Program.Settings.Sonarr.Enabled)
            {
                var series = Program.ServiceManager.Sonarr.GetAllSeries();
                if (series != null)
                {
                    _seriesCache = series.ConvertAll((o) => new BasicSeries(o));
                }
            }

            if (Program.Settings.Lidarr.Enabled)
            {
                var artists = Program.ServiceManager.Lidarr.GetAllArtists();
                if (artists != null)
                {
                    _artistCache = artists.ConvertAll((o) => new BasicArtist(o));
                }
            }

            _cachedResponse = null;
            _nextCacheUpdate = DateTime.Now.AddMinutes(_cacheDuration);

            _logger.Trace("Successfully updated cache");
        }
    }
}
