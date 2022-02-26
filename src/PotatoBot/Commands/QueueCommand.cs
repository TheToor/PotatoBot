using ByteSizeLib;
using PotatoBot.API;
using PotatoBot.Modals;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Requests.DELETE;
using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("queue", Description = "Shows the current download queue")]
    internal class QueueCommand : Service, ICommand, IQueryCallback
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const string DataRemoveDownload = "RemoveDownload";
        private const string DataRemoveAndBlock = "RemoveAndBlock";

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var keyboardMarkup = TelegramService.GetDefaultEntertainmentInlineKeyboardButtons();
            if(keyboardMarkup.Count == 0)
            {
                // Nothing enabled so nothing to search for
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return true;
            }

            if(arguments.Length == 0 || arguments.Length != 2)
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

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            var messageData = callbackQuery.Data;
            var message = callbackQuery.Message;

            await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

            if(messageData.StartsWith(DataRemoveDownload) || messageData.StartsWith(DataRemoveAndBlock))
            {
                return await ProcessDeleteCallbackQuery(client, messageData, message);
            }
            return await ProcessQueueCallbackQuery(client, messageData, message);
        }

        private static string GetQueueItemCompletion(QueueItem queueItem)
        {
            var completion = Math.Floor(100f / queueItem.Size * (queueItem.Size - queueItem.SizeLeft)).ToString("000");
            if(completion == double.NaN.ToString())
            {
                completion = "000";
            }
            return completion;
        }

        private async Task SendQueueItemStatus(Message message, string[] arguments)
        {
            var service = Program.ServiceManager.GetAllServices().FirstOrDefault(s => s.Name == arguments[0]);
            if(service == null || service is not APIBase api)
            {
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return;
            }
            if(!uint.TryParse(arguments[1], out var queueItemId))
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

            var sizeLeft = ByteSize.FromBytes(queueItem.Size - queueItem.SizeLeft);
            var size = ByteSize.FromBytes(queueItem.Size);

            var text = $"\n<b>[{GetQueueItemCompletion(queueItem)}%][{queueItem.Status}]</b> {queueItem.GetQueueTitle()}\n";
            text += $"Indexer: {queueItem.Indexer}\n";
            text += $"Download: {(int)Math.Round(sizeLeft.LargestWholeNumberBinaryValue)} {sizeLeft.LargestWholeNumberBinarySymbol}/{(int)Math.Round(size.LargestWholeNumberBinaryValue)} {size.LargestWholeNumberBinarySymbol}\n";
            text += $"Estimated Completion Time: {queueItem.EstimatedCompletionTime}\n";

            if(queueItem.TrackedDownloadStatus != TrackedDownloadStatus.Ok)
            {
                text += "\n";
                foreach(var statusMessage in queueItem.StatusMessages)
                {
                    text += $"<b>{statusMessage.Title}</b>\n";
                    text += string.Join("\n", statusMessage.Messages);
                }
            }

            var keyboardMarkup = new InlineKeyboardMarkup(
                new List<List<InlineKeyboardButton>>()
                {
                    new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData(Program.LanguageManager.GetTranslation("RemoveDownload"), $"{DataRemoveDownload}_{service.Name}_{queueItemId}")
                    },
                    new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData(Program.LanguageManager.GetTranslation("RemoveAndBlock"), $"{DataRemoveAndBlock}_{service.Name}_{queueItemId}")
                    }
                }
            );

            await TelegramService.ReplyWithMarkup(this, message, text, keyboardMarkup, ParseMode.Html);
        }

        private static async Task<bool> ProcessDeleteCallbackQuery(TelegramBotClient client, string messageData, Message message)
        {
            var split = messageData.Split('_');
            if(split.Length != 3)
            {
                _logger.Warn($"Invalid split. Expected 2 but got {split.Length}");
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", "DeleteFailed"));
                return false;
            }

            var service = Program.ServiceManager.GetAllServices().FirstOrDefault(s => s.Name == split[1]);
            if(service == null || service is not APIBase api)
            {
                _logger.Warn($"Failed to find Service with name '{split[1]}'");
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", "DeleteFailed"));
                return false;
            }

            if(!uint.TryParse(split[2], out var queueItemId))
            {
                _logger.Warn($"Failed to parse '{split[2]}' to valid queue item id");
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", "DeleteFailed"));
                return false;
            }

            if(!api.RemoveFromQueue(new RemoveQueueItem(queueItemId, true, messageData.StartsWith(DataRemoveAndBlock))))
            {
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", "DeleteFailed"));
                return false;
            }

            await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", "DeleteSuccess"));
            return true;
        }

        private static async Task<bool> ProcessQueueCallbackQuery(TelegramBotClient client, string messageData, Message message)
        {
            if(!Enum.TryParse<ServarrType>(messageData, out var searchType))
            {
                _logger.Warn($"Failed to parse {messageData} to {nameof(ServarrType)}");
                return true;
            }

            if(searchType == ServarrType.Unknown)
            {
                _logger.Warn($"User {message.Chat.Username} somehow wants to show the queue for nothing.");
                return true;
            }

            var searchTypeString = searchType.ToString();
            var queue = Program.ServiceManager.GetAllServices().Where(s => s is IServarr apiBase && apiBase.Type == searchType).Cast<APIBase>().SelectMany(s => s.GetQueue()).ToList();

            if(queue?.Count == 0)
            {
                await client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("Commands", "Queue", $"No{searchTypeString}"));
            }
            else
            {
                var queueText = string.Format($"{Program.LanguageManager.GetTranslation("Commands", "Queue", searchTypeString)}\n\n", queue.Count);
                var grouped = queue.GroupBy(q => q.TrackedDownloadStatus);

                foreach(var group in grouped)
                {
                    queueText += $"<b>{group.Key}</b>\n";
                    foreach(var item in group)
                    {
                        queueText += $"<b>[{GetQueueItemCompletion(item)}%][{item.Status}]</b> {item.GetQueueTitle()} /queue_{item.API.Name}_{item.Id}\n";
                    }
                }

                await TelegramService.SendSimpleMessage(message.Chat.Id, queueText, ParseMode.Html);
            }

            return true;
        }
    }
}
