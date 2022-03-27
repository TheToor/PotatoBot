using System;
using System.Linq;
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

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task Send(TelegramBotClient client, Message message, bool create, Cache cache, PageResult<IServarrItem> page);
        public async Task<bool> HandlePagination(TelegramBotClient client, Message message, Cache cache, string data)
        {
            if(data == DisabledData)
            {
                _logger.Trace("Stupid user clicked on disabled button ... Ignoring the twat");
                return true;
            }

            if(data == NextData || data == PreviousData || data == NextFiveData || data == PreviousFiveData)
            {
                if(data == NextData)
                {
                    cache.Page++;
                }
                else if(data == PreviousData)
                {
                    cache.Page--;
                }
                else if(data == NextFiveData)
                {
                    cache.Page += Math.Min(cache.PageItemList.Count() - cache.Page, 5);
                }
                else
                {
                    cache.Page -= Math.Min(cache.Page, 5);
                }

                await Program.ServiceManager.TelegramService.UpdatePageination(message);

                // Do not invoke any further tasks
                return true;
            }

            if(data == CancelData)
            {
                // Cancel Pagination
                _logger.Trace("Cancellation requested");

                await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                return true;
            }

            if(data.StartsWith(SelectData))
            {
                var selection = data.Substring(SelectData.Length, data.Length - SelectData.Length);
                if(!int.TryParse(selection, out var selectedIndex))
                {
                    // How ?
                    _logger.Warn($"Failed to parse '{selection}' to int");
                    await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }

                try
                {
                    var task = cache.PageSelectionFunction(client, message, selectedIndex);
                    task.Wait();

                    if(!task.Result)
                    {
                        _logger.Warn("Failed to process selection");
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to execute selection function");
                    await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }

                return true;
            }

            return false;
        }
    }
}
