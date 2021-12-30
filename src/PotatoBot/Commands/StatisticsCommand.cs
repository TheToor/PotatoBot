using PotatoBot.Modals.Commands;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("stats", Description = "Shows statistics of the bot")]
	internal class StatisticsCommand : Service, ICommand
	{
		public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
		{
			var statistics = Services.StatisticsService.GetCurrentStatistics();
			var responseText = string.Format(
				LanguageManager.GetTranslation("Commands", "Stats", "Stats"),
				statistics.MessagesSent,
				statistics.MessagesReveived,
				statistics.MessagesProcessed,
				statistics.CommandsReceived,
				statistics.CommandsProcessed,
				statistics.Searches,
				statistics.Adds,
				statistics.WebhooksReceived,
				statistics.WebhooksProcessed
			);

			await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

			return true;
		}
	}
}
