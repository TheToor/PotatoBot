using Microsoft.AspNetCore.Mvc;
using NLog;
using PotatoBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PotatoBot.Controllers.Webhook
{
    [Route("webhook/[controller]", Name = "Plex")]
    public class PlexController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly StatisticsService _statisticsService;

        public PlexController(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        private bool ValidateRequest()
        {
            var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "User-Agent").Value.First();
            if(Request.ContentType == null)
            {
                _logger.Trace($"Invalid content type (null / empty)");
                return false;
            }
            var split = Request.ContentType.Split(';');
            if(split.Length == 0)
            {
                _logger.Trace($"Invalid content type ({Request.ContentType})");
                return false;
            }

            var contentType = split[0];
            var method = Request.Method;

            _logger.Trace($"Request from {Request.Host} ( '{userAgent}' / '{contentType}' / {method} )");

            if(contentType != "multipart/form-data")
            {
                _logger.Warn("Invalid content type");
                return false;
            }

            if(method != "POST")
            {
                _logger.Warn("Not a POST request");
                return false;
            }

            return true;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            await _statisticsService.Increase(Modals.TrackedStatistics.WebhooksReceived);

            if(!ValidateRequest())
            {
                return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
            }

            using var streamReader = new StreamReader(Request.Body);
            try
            {
                var content = streamReader.ReadToEnd();
                if(content.Contains("name=\"thumb\";"))
                {
                    // We don't care about the thumbnail
                    _logger.Trace($"Skipping request as thumbnail was detected");
                    await _statisticsService.Increase(Modals.TrackedStatistics.WebhooksProcessed);
                    return new StatusCodeResult((int)HttpStatusCode.OK);
                }

                var start = content.IndexOf('{');
                var end = content.LastIndexOf('}');

                if(start == -1 || end == -1)
                {
                    _logger.Trace($"Skipping requests ({start}/{end})");
                    await _statisticsService.Increase(Modals.TrackedStatistics.WebhooksProcessed);
                    return new StatusCodeResult((int)HttpStatusCode.OK);
                }

                await _statisticsService.Increase(Modals.TrackedStatistics.WebhooksProcessed);
                return new StatusCodeResult((int)HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, "Failed to process request");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
