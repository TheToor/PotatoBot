using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("version", Description = "Shows current version of the PotatoBot")]
    public class VersionCommand : ICommand
    {
        private readonly TelegramService _telegramService;

        public VersionCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            await _telegramService.SimpleReplyToMessage(message, $"Version {Program.Version} of {Program.Namespace}");
            return true;
        }
    }
}
