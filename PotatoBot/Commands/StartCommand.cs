using PotatoBot.Modals.Commands;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("start")]
    internal class StartCommand : Service, ICommand
    {
        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Start", "Start"));
            return true;
        }
    }
}
