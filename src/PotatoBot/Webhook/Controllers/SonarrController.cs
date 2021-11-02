using ByteSizeLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using PotatoBot.Webhook.Modals;
using PotatoBot.Webhook.Modals.Sonarr;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PotatoBot.Webhook.Controllers
{
	[Route("webhook/[controller]", Name = "Sonarr")]
	public class SonarrController : Controller
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private static Services.TelegramService _telegramManager => Program.ServiceManager.TelegramService;

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
			if(server != "Sonarr")
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

			if(!ValidateRequest())
			{
				return new StatusCodeResult((int)HttpStatusCode.NotAcceptable);
			}

			using(var streamReader = new StreamReader(Request.Body))
			{
				var json = streamReader.ReadToEnd();

				var responseBase = JsonConvert.DeserializeObject<RequestBase>(json);
				switch(responseBase.EventType)
				{
					case EventType.Grab:
						{
							var grabEvent = JsonConvert.DeserializeObject<Grab>(json);
							var size = ByteSize.FromBytes(grabEvent.Release.Size);

							var episodes = string.Empty;
							foreach(var episode in grabEvent.Episodes)
							{
								episodes += $"\n[S{episode.SeasonNumber.ToString("00")}E{episode.EpisodeNumber.ToString("00")}] {episode.Title}";
							}

							await _telegramManager.SendToAll(
								string.Format(
									Program.LanguageManager.GetTranslation("Series", "Grab"),
									grabEvent.Series.Title,
									episodes,
									grabEvent.Release.Quality,
									grabEvent.Release.ReleaseGroup,
									$"{Math.Round(size.LargestWholeNumberBinaryValue, 2):0.00} {size.LargestWholeNumberBinarySymbol}"
								)
							);
							break;
						}

					case EventType.Download:
						{
							var downloadEvent = JsonConvert.DeserializeObject<DownloadUpgrade>(json);

							var eventType = "Download";
							if(downloadEvent.IsUpgrade)
							{
								eventType = "Upgrade";
							}

							var episodes = string.Empty;
							foreach(var episode in downloadEvent.Episodes)
							{
								episodes += $"\n[S{episode.SeasonNumber.ToString("00")}E{episode.EpisodeNumber.ToString("00")}] {episode.Title}";
							}

							await _telegramManager.SendToAll(
								string.Format(
									Program.LanguageManager.GetTranslation("Series", eventType),
									downloadEvent.Series.Title,
									episodes
								)
							);

							break;
						}

					case EventType.Rename:
						{
							var renameEvent = JsonConvert.DeserializeObject<Rename>(json);
							await _telegramManager.SendToAll(
								string.Format(
									Program.LanguageManager.GetTranslation("Series", "Rename"),
									renameEvent.Series.Title
								)
							);

							break;
						}

					case EventType.Test:
						{
							var testEvent = JsonConvert.DeserializeObject<Test>(json);

							var episodes = string.Empty;
							foreach(var episode in testEvent.Episodes)
							{
								episodes += $"\n[S{episode.SeasonNumber.ToString("00")}E{episode.EpisodeNumber.ToString("00")}] {episode.Title}";
							}

							await _telegramManager.SendToAll(
								string.Format(
									Program.LanguageManager.GetTranslation("Series", "Test"),
									testEvent.Series.Title,
									episodes
								)
							);
							break;
						}
				}

				Program.ServiceManager.StatisticsService.IncreaseWebhooksProcessed();
				return new StatusCodeResult((int)HttpStatusCode.OK);
			}
		}
	}
}
