using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Model.Commands
{
    public interface IQueryCallback
    {
        Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery);
    }
}
