using ByteSizeLib;
using PotatoBot.API;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("queue", Description = "Shows the current download queue")]
    internal class QueueCommand : Service, ICommand, IQueryCallback
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var keyboardMarkup = TelegramService.GetDefaultEntertainmentInlineKeyboardButtons();
            if (keyboardMarkup.Count == 0)
            {
                // Nothing enabled so nothing to search for
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return true;
            }

            if (arguments.Length == 0 || arguments.Length != 2)
            {
                var markup = new InlineKeyboardMarkup(keyboardMarkup);
                var title = LanguageManager.GetTranslation("Commands", "Queue", "Start");
                await TelegramService.ReplyWithMarkup(this, message, title, markup);
            }
            else
            {
                await SendQueueItemStatus(message, arguments);
            }

            return true;
        }

        private static async Task SendQueueItemStatus(Message message, string[] arguments)
        {
            var service = Program.ServiceManager.GetAllServices().FirstOrDefault(s => s.Name == arguments[0]);
            if(service == null || service is not APIBase api)
            {
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return;
            }
            if (!uint.TryParse(arguments[1], out var queueItemId))
            {
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return;
            }

            var queueItem = api.GetQueue().FirstOrDefault(i => i.Id == queueItemId);
            if(queueItem == null)
            {
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return;
            }

            var completion = Math.Floor(100f / queueItem.Size * (queueItem.Size - queueItem.SizeLeft)).ToString("000");
            if (completion == double.NaN.ToString())
            {
                completion = "000";
            }

            var sizeLeft = ByteSize.FromBytes(queueItem.Size - queueItem.SizeLeft);
            var size = ByteSize.FromBytes(queueItem.Size);

            var text = $"\n\t<b>[{completion}%][{queueItem.Status}]</b>{queueItem.GetQueueTitle()}\n";
            text += $"Indexer: {queueItem.Indexer}\n";
            text += $"{(int)Math.Round(sizeLeft.LargestWholeNumberBinaryValue)} {sizeLeft.LargestWholeNumberBinarySymbol}/{(int)Math.Round(size.LargestWholeNumberBinaryValue)} {size.LargestWholeNumberBinarySymbol}\n";
            text += $"Estimated Completion Time: {queueItem.EstimatedCompletionTime}\n";
            await TelegramService.SendSimpleMessage(message.Chat.Id, text, Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            var messageData = callbackQuery.Data;
            var message = callbackQuery.Message;
            var cacheData = TelegramService.GetCachedData<SearchData>(message);

            await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            if (!Enum.TryParse<SearchType>(messageData, out var searchType))
            {
                _logger.Warn($"Failed to parse {messageData} to {nameof(SearchType)}");
                return true;
            }

            if (searchType == SearchType.None)
            {
                _logger.Warn($"User {message.Chat.Username} somehow wants to show the queue for nothing.");
                return true;
            }

            var searchTypeString = searchType.ToString();
            var queue = Program.ServiceManager.GetAllServices().Where(s => s is APIBase).Cast<APIBase>().SelectMany(s => s.GetQueue()).ToList();

            if (queue?.Count == 0)
            {
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", $"No{searchTypeString}"));
            }
            else
            {
                var queueText = string.Format($"{Program.LanguageManager.GetTranslation("Commands", "Queue", searchTypeString)}\n", queue.Count);

                foreach (var item in queue)
                {
                    var completion = Math.Floor(100f / item.Size * (item.Size - item.SizeLeft)).ToString("000");
                    if (completion == Double.NaN.ToString())
                    {
                        completion = "000";
                    }

                    queueText += $"\n\t<b>[{completion}%][{item.Status}]</b>{item.GetQueueTitle()} /queue_{item.API.Name}_{item.Id}";
                }

                await TelegramService.SendSimpleMessage(message.Chat.Id, queueText, Telegram.Bot.Types.Enums.ParseMode.Html);
            }

            return true;
        }
    }
}
