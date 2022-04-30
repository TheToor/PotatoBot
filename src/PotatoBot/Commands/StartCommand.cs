using PotatoBot.Model.Commands;
using PotatoBot.Services;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("start", Description = "Well it's a start isn't it?")]
    public class StartCommand : ICommand
    {
        private readonly TelegramService _telegramService;
        private readonly LanguageService _languageManager;

        public StartCommand(TelegramService telegramService, LanguageService languageManager)
        {
            _telegramService = telegramService;
            _languageManager = languageManager;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            await _telegramService.SimpleReplyToMessage(message, _languageManager.GetTranslation("Commands", "Start", "Start"));
            return true;
        }
    }
}
