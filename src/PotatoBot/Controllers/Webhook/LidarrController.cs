using ByteSizeLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PotatoBot.Modals;
using PotatoBot.Modals.Webhook;
using PotatoBot.Services;
using PotatoBot.Webhook.Modals.Lidarr;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PotatoBot.Controllers.Webhook
{
    [Route("webhook/[controller]", Name = "Lidarr")]
    public class LidarrController : Controller
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TelegramService _telegramService;
        private readonly StatisticsService _statisticsService;
        private readonly ServiceManager _serviceManager;
        private readonly LanguageService _languageManager;

        public LidarrController(TelegramService telegramService, StatisticsService statisticsService, ServiceManager serviceManager, LanguageService languageManager)
        {
            _telegramService = telegramService;
            _statisticsService = statisticsService;
            _serviceManager = serviceManager;
            _languageManager = languageManager;
        }

        private bool ValidateRequest()
        {
            var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "User-Agent").Value.First();
            var contentType = Request.ContentType;
            var method = Request.Method;

            _logger.Trace($"Request from {Request.Host} ( '{userAgent}' / '{contentType}' / {method} )");

            if(contentType != "application/json")
            {
                _logger.Warn("Not a json request");
                return false;
            }

            if(method != "POST")
            {
                _logger.Warn("Not a POST request");
                return false;
            }

            var server = userAgent.Split("/")[0];
            if(server != "Lidarr")
            {
                _logger.Warn("Invalid request");
                return false;
            }

            return true;
        }

        [Route("{serviceName}")]
        [HttpPost]
        public async Task<IActionResult> Index(string serviceName)
        {
            _statisticsService.IncreaseWebhooksReceived();

            if(!ValidateRequest())
            {
                return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
            }

            var service = _serviceManager.GetAllServices().FirstOrDefault(s => s is IServarr && s.Name == serviceName);
            if(service == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            using(var streamReader = new StreamReader(Request.Body))
            {
                var json = streamReader.ReadToEnd();

                var responseBase = JsonConvert.DeserializeObject<LidarrRequestBase>(json);
                switch(responseBase.EventType)
                {
                    case EventType.Grab:
                    {
                        var grabEvent = JsonConvert.DeserializeObject<Grab>(json);
                        var size = ByteSize.FromBytes(grabEvent.Release.Size);

                        await _telegramService.SendToAll(
                            string.Format(
                                _languageManager.GetTranslation("Artists", "Grab"),
                                grabEvent.Albums.Select((a) => a.Title).Aggregate((i, j) => i + "\n" + j),
                                grabEvent.Release.Quality,
                                grabEvent.Release.ReleaseGroup,
                                $"{Math.Round(size.LargestWholeNumberBinaryValue, 2):0.00} {size.LargestWholeNumberBinarySymbol}",
                                service.Name,
                                grabEvent.Artist.Id
                            )
                        );
                    }
                    break;

                    case EventType.Download:
                    {
                        var downloadEvent = JsonConvert.DeserializeObject<Download>(json);

                        var eventType = "Download";
                        if(downloadEvent.IsUpgrade)
                        {
                            eventType = "Upgrade";
                        }

                        await _telegramService.SendToAll(
                            string.Format(
                                _languageManager.GetTranslation("Artists", eventType),
                                downloadEvent.Tracks.Select((t) => t.Title).Aggregate((i, j) => i + "\n" + j),
                                service.Name,
                                downloadEvent.Artist.Id
                            )
                        );
                    }
                    break;

                    case EventType.Rename:
                    {
                        await _telegramService.SendToAll(
                            string.Format(
                                _languageManager.GetTranslation("Artists", "Rename"),
                                responseBase.Artist.Name
                            )
                        );
                    }
                    break;

                    case EventType.Retag:
                    {
                        await _telegramService.SendToAll(
                            string.Format(
                                _languageManager.GetTranslation("Artists", "Retag"),
                                responseBase.Artist.Name
                            )
                        );
                    }
                    break;

                    case EventType.Test:
                    {
                        var testEvent = JsonConvert.DeserializeObject<Test>(json);
                        await _telegramService.SendToAll(
                            string.Format(
                                _languageManager.GetTranslation("Artists", "Test"),
                                testEvent.Albums.Select((a) => a.Title).Aggregate((i, j) => i + "\n" + j)
                            )
                        );
                    }
                    break;
                }
            }

            _statisticsService.IncreaseWebhooksProcessed();
            return new StatusCodeResult((int)HttpStatusCode.OK);
        }
    }
}
