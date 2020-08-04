using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PotatoBot.API
{
    [Route("[controller]", Name = "Preview")]
    public class Preview : Controller
    {
        internal static Dictionary<string, object> CachedResponse;

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            if(CachedResponse == null)
            {
                return Json(Array.Empty<string>());
            }
            return Json(CachedResponse);
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
    }
}
