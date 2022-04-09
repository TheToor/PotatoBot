using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("help", Description = "Maybe shows a help")]
    public class HelpCommand : ICommand
    {
        private readonly TelegramService _telegramService;
        private readonly LanguageService _languageManager;

        public HelpCommand(TelegramService telegramService, LanguageService languageManager)
        {
            _telegramService = telegramService;
            _languageManager = languageManager;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            await _telegramService.SimpleReplyToMessage(message, _languageManager.GetTranslation("Commands", "Help", "Help"));
            return true;
        }
    }
}
