using PotatoBot.Model.Commands;
using PotatoBot.Services;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("stats", Description = "Shows statistics of the bot")]
    public class StatisticsCommand : ICommand
    {
        private readonly TelegramService _telegramService;
        private readonly LanguageService _languageManager;
        private readonly StatisticsService _statisticsService;

        public StatisticsCommand(TelegramService telegramService, LanguageService languageManager, StatisticsService statisticsService)
        {
            _telegramService = telegramService;
            _languageManager = languageManager;
            _statisticsService = statisticsService;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var statistics = _statisticsService.GetStatistics();
            if (statistics == null)
            {
                return false;
            }

            var responseText = string.Format(
                _languageManager.GetTranslation("Commands", "Stats", "Stats"),
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

            await _telegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }
    }
}
