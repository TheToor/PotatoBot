using PotatoBot.Modals.API;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
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

            var markup = new InlineKeyboardMarkup(keyboardMarkup);
            var title = LanguageManager.GetTranslation("Commands", "Queue", "Start");
            await TelegramService.ReplyWithMarkup(this, message, title, markup);
            return true;
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQueryEventArgs e)
        {
            var messageData = e.CallbackQuery.Data;
            var message = e.CallbackQuery.Message;
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
            var queue = new List<QueueItem>();

            switch(searchType)
            {
                case SearchType.Series:
                    {
                        queue = SonarrService.GetQueue().Cast<QueueItem>().ToList();
                    }
                    break;

                case SearchType.Movie:
                    {
                        queue = RadarrService.GetQueue().Cast<QueueItem>().ToList();
                    }
                    break;

                case SearchType.Artist:
                    {
                        queue = LidarrService.GetQueue().Cast<QueueItem>().ToList();
                    }
                    break;
            }

            if(queue?.Count == 0)
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

                    queueText += $"\n\t<b>[{completion}%][{item.Status}]</b> {item.GetQueueTitle()}";
                }

                await TelegramService.SendSimpleMessage(message.Chat.Id, queueText, Telegram.Bot.Types.Enums.ParseMode.Html);
            }

            return true;
        }
    }
}
