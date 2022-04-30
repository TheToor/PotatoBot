using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Model.Commands
{
    internal interface ICommand
    {
        Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments);
    }
}
