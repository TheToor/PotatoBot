using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PotatoBot.Modals.Commands
{
    public interface IQueryCallback
    {
        Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQueryEventArgs e);
    }
}
