using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ByteSizeLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using PotatoBot.Webhook.Modals;
using PotatoBot.Webhook.Modals.Lidarr;

namespace PotatoBot.Webhook.Controllers
{
    [Route("webhook/[controller]", Name = "Lidarr")]
    public class LidarrController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static Services.TelegramService _telegramManager => Program.ServiceManager.TelegramService;

        private bool ValidateRequest()
        {
            var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "User-Agent").Value.First();
            var contentType = Request.ContentType;
            var method = Request.Method;

            _logger.Trace($"Request from {Request.Host} ( '{userAgent}' / '{contentType}' / {method} )");

            if (contentType != "application/json")
            {
                _logger.Warn("Not a json request");
                return false;
            }

            if (method != "POST")
            {
                _logger.Warn("Not a POST request");
                return false;
            }

            var server = userAgent.Split("/")[0];
            if (server != "Lidarr")
            {
                _logger.Warn("Invalid request");
                return false;
            }

            return true;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            Program.ServiceManager.StatisticsService.IncreaseWebhooksReceived();

            if (!ValidateRequest())
            {
                return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
            }

            using (var streamReader = new StreamReader(Request.Body))
            {
                var json = streamReader.ReadToEnd();

                var responseBase = JsonConvert.DeserializeObject<LidarrRequestBase>(json);
                switch(responseBase.EventType)
                {
                    case EventType.Grab:
                        {
                            var grabEvent = JsonConvert.DeserializeObject<Grab>(json);
                            var size = ByteSize.FromBytes(grabEvent.Release.Size);

                            await _telegramManager.SendToAll(
                                string.Format(
                                    Program.LanguageManager.GetTranslation("Artists", "Grab"),
                                    grabEvent.Albums.Select((a) => a.Title).Aggregate((i, j) => i + "\n" + j),
                                    grabEvent.Release.Quality,
                                    grabEvent.Release.ReleaseGroup,
                                    $"{Math.Round(size.LargestWholeNumberDecimalValue, 2):0.00} {size.LargestWholeNumberDecimalSymbol}"
                                )
                            );
                        }
                        break;

                    case EventType.Download:
                        {
                            var downloadEvent = JsonConvert.DeserializeObject<Download>(json);

                            var eventType = "Download";
                            if (downloadEvent.IsUpgrade)
                            {
                                eventType = "Upgrade";
                            }

                            await _telegramManager.SendToAll(
                                string.Format(
                                    Program.LanguageManager.GetTranslation("Artists", eventType),
                                    downloadEvent.Tracks.Select((t) => t.Title).Aggregate((i, j) => i + "\n" + j)
                                )
                            );
                        }
                        break;

                    case EventType.Rename:
                        {
                            await _telegramManager.SendToAll(
                                string.Format(
                                    Program.LanguageManager.GetTranslation("Artists", "Rename"),
                                    responseBase.Artist.Name
                                )
                            );
                        }
                        break;

                    case EventType.Retag:
                        {
                            await _telegramManager.SendToAll(
                                string.Format(
                                    Program.LanguageManager.GetTranslation("Artists", "Retag"),
                                    responseBase.Artist.Name
                                )
                            );
                        }
                        break;

                    case EventType.Test:
                        {
                            var testEvent = JsonConvert.DeserializeObject<Test>(json);
                            await _telegramManager.SendToAll(
                                string.Format(
                                    Program.LanguageManager.GetTranslation("Artists", "Test"),
                                    testEvent.Albums.Select((a) => a.Title).Aggregate((i, j) => i + "\n" + j)
                                )
                            );
                        }
                        break;
                }
            }

            Program.ServiceManager.StatisticsService.IncreaseWebhooksProcessed();
            return new StatusCodeResult((int)HttpStatusCode.OK);
        }
    }
}
