using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Modals.Commands
{
	public interface IReplyCallback
	{
		public string UniqueIdentifier { get; }
		Task<bool> OnReplyReceived(TelegramBotClient client, Message message);
	}
}
