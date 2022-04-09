using Microsoft.AspNetCore.Mvc;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PotatoBot.Controllers
{
    [Route("[controller]", Name = "Preview")]
    public class Preview : Controller
    {
        internal static Dictionary<string, object> CachedPreviewResponse;
        internal static Dictionary<string, ulong> CachedStatisticsResponse;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BotSettings _botSettings;

        public Preview(BotSettings botSettings)
        {
            _botSettings = botSettings;
        }

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            if(CachedPreviewResponse == null)
            {
                return Json(Array.Empty<string>());
            }
            return Json(CachedPreviewResponse);
        }

        [Route("poster/{type}")]
        [HttpGet]
        public async Task<IActionResult> GetPoster(string type, [FromQuery(Name = "url")] string requestUrl)
        {
            var libraryUrl = string.Empty;
            switch(type)
            {
                case "movie":
                    libraryUrl = _botSettings.Radarr.FirstOrDefault(r => r.Enabled)?.Url;
                    break;

                case "series":
                    libraryUrl = _botSettings.Sonarr.FirstOrDefault(s => s.Enabled)?.Url;
                    break;

                case "artist":
                    libraryUrl = _botSettings.Lidarr.FirstOrDefault(l => l.Enabled)?.Url;
                    break;
            }

            if(libraryUrl == null)
            {
                return NotFound();
            }

            var imageBytes = await DownloadPoster(libraryUrl, requestUrl);

            return File(imageBytes, "image/jpeg");
        }

        [Route("mediastatistics")]
        [HttpGet]
        public IActionResult GetMediaStatistics()
        {
            if(CachedStatisticsResponse == null)
            {
                return Json(Array.Empty<string>());
            }
            return Json(CachedStatisticsResponse);
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
                if(response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                _logger.Warn($"Failed to fetch poster from '{url}'");
                return Array.Empty<byte>();
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"Failed to fetch poster from '{url}'");
                return Array.Empty<byte>();
            }
        }
    }
}
