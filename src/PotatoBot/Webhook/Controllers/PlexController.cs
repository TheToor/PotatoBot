using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace PotatoBot.Webhook.Controllers
{
	[Route("webhook/[controller]", Name = "Plex")]
	public class PlexController : Controller
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private bool ValidateRequest()
		{
			var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "User-Agent").Value.First();
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
		public IActionResult Index()
		{
			Program.ServiceManager.StatisticsService.IncreaseWebhooksReceived();

			if(!ValidateRequest())
			{
				return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
			}

			using(var streamReader = new StreamReader(Request.Body))
			{
				try
				{
					var content = streamReader.ReadToEnd();
					if(content.Contains("name=\"thumb\";"))
					{
						// We don't care about the thumbnail
						_logger.Trace($"Skipping request as thumbnail was detected");
						Program.ServiceManager.StatisticsService.IncreaseWebhooksProcessed();
						return new StatusCodeResult((int)HttpStatusCode.OK);
					}

					var start = content.IndexOf('{');
					var end = content.LastIndexOf('}');

					if(start == -1 || end == -1)
					{
						_logger.Trace($"Skipping requests ({start}/{end})");
						Program.ServiceManager.StatisticsService.IncreaseWebhooksProcessed();
						return new StatusCodeResult((int)HttpStatusCode.OK);
					}

					//                    var json = content.Substring(start, end - start + 1).Trim();
					//#if DEBUG
					//                    _logger.Trace(json);
					//#endif
					//                    var plexEventBase = Newtonsoft.Json.JsonConvert.DeserializeObject<PlexEventBase>(json);
					//                    if(plexEventBase != null && plexEventBase.EventType == EventType.NewInLibrary)
					//                    {
					//                        ProcessRequest(Newtonsoft.Json.JsonConvert.DeserializeObject<PlexEvent>(json));
					//                    }

					Program.ServiceManager.StatisticsService.IncreaseWebhooksProcessed();
					return new StatusCodeResult((int)HttpStatusCode.OK);
				}
				catch(Exception ex)
				{
					_logger.Warn(ex, "Failed to process request");
					return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
				}
			}
		}

		//private static void ProcessRequest (PlexEvent plexEvent)
		//{

		//}
	}
}
