using PotatoBot.Modals.Commands;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("version")]
    internal class VersionCommand : Service, ICommand
    {
        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            await TelegramService.SimpleReplyToMessage(message, $"Version {Program.Version} of {Program.Namespace}");
            return true;
        }
    }
}
