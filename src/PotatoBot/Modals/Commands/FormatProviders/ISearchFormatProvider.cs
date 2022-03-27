using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Modals.Commands.FormatProviders
{
    public interface ISearchFormatProvider
    {
        public const string PreviousData = "Previous";
        public const string PreviousFiveData = "Previous5";
        public const string NextData = "Next";
        public const string NextFiveData = "Next5";
        public const string SelectData = "Select";
        public const string DisabledData = "Disabled";
        public const string CancelData = "Cancel";

        public const string Spacer = "  ";

        public const string MissingImageUrl = "https://thetvdb.com/images/missing/movie.jpg";

        public Task Send(TelegramBotClient client, Message message, bool create, Cache cache, PageResult<IServarrItem> page);
    }
}
