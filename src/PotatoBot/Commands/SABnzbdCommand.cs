using PotatoBot.Managers;
using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
	[Command("sab", Description = "Controls SAB servers")]
	internal class SABnzbdCommand : Service, ICommand
	{
		internal enum SABnzbdCommandMode
		{
			Status,
			Pause,
			Resume,
			Delete
		}

		public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
		{
			if(
				arguments.Length == 0 ||
				!Enum.TryParse(arguments[0], true, out SABnzbdCommandMode command) ||
				( command == SABnzbdCommandMode.Delete && arguments.Length != 2 )
			)
			{
				await TelegramService.SimpleReplyToMessage(
					message,
					LanguageManager.GetTranslation("Commands", "SABnzbd", "CommandNotFound")
				);
				return true;
			}

			var servers = Program.ServiceManager.GetSABnzbdServices();
			return command switch
			{
				SABnzbdCommandMode.Status => await ProcessStatusCommand(message, servers),
				SABnzbdCommandMode.Pause => await ProcessPauseCommand(message, servers),
				SABnzbdCommandMode.Resume => await ProcessResumeCommand(message, servers),
				SABnzbdCommandMode.Delete => await ProcessDeleteCommand(message, servers, arguments[1]),
				_ => true,
			};
		}

		private static async Task<bool> ProcessStatusCommand(Message message, List<SABnzbdService> servers)
		{
			var responseText = string.Format(
				LanguageManager.GetTranslation("Commands", "SABnzbd", "Status", "Title"),
				servers.Count
			);

			foreach(var server in servers)
			{
				// Queue returns queue and fullstatus
				var response = await server.GetQueue();
				if(response == null)
				{
					continue;
				}

				// response.Status if using server.GetStatus() !
				var queue = response.Queue;

				responseText += $"<b>{server.Name}</b>\n";
				responseText += string.Format(
					LanguageManager.GetTranslation("Commands", "SABnzbd", "Status", "Text"),
					queue.Paused ? queue.Paused : queue.PausedAll,
					queue.Version,
					queue.LoadAverage,
					queue.Diskspace1Norm,
					queue.SizeLeft,
					queue.TimeLeft,
					queue.EstimatedRemainingTime
				);

				var status = await server.GetStatus();
				if(status.Status?.Folders?.Count > 0)
				{
					responseText += "\n\n<b>Orphaned Jobs</b>\n";
					responseText += string.Join("\n", status.Status.Folders);
					responseText += $"\n/sab_delete_{server.Name}";
				}
				responseText += "\n\n";
			}

			await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

			return true;
		}

		private static async Task<bool> ProcessPauseCommand(Message message, List<SABnzbdService> servers)
		{
			var responseText = string.Format(
				LanguageManager.GetTranslation("Commands", "SABnzbd", "Pause"),
				servers.Count
			);

			foreach(var server in servers)
			{
				var response = await server.PauseQueue();

				responseText += $"{server.Name}: {response} \n";
			}

			await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

			return true;
		}

		private static async Task<bool> ProcessResumeCommand(Message message, List<SABnzbdService> servers)
		{
			var responseText = string.Format(
				LanguageManager.GetTranslation("Commands", "SABnzbd", "Resume"),
				servers.Count
			);

			foreach(var server in servers)
			{
				var response = await server.ResumeQueue();

				responseText += $"{server.Name}: {response} \n";
			}

			await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

			return true;
		}

		private static async Task<bool> ProcessDeleteCommand(Message message, List<SABnzbdService> servers, string server)
		{
			var selectedServer = servers.FirstOrDefault(s => s.Name == server);
			if(selectedServer == null)
			{
				return false;
			}

			if(await selectedServer.DeleteOrphanedQueue())
			{
				await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "SABnzbd", "DeleteSuccess"));
			}
			else
			{
				await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "SABnzbd", "DeleteFailed"));
			}
			return true;
		}
	}
}
