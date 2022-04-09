using PotatoBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Modals.Commands.FormatProviders
{
    public class ListSearchFormatProvider : ISearchFormatProvider
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly StatisticsService _statisticsService;
        private readonly LanguageService _languageManager;
        public ListSearchFormatProvider(StatisticsService statisticsService, LanguageService languageManager)
        {
            _statisticsService = statisticsService;
            _languageManager = languageManager;
        }

        public async Task Send(TelegramBotClient client, Message message, bool create, Cache cache, PageResult<IServarrItem> page)
        {
            var keyboardMarkupData = new List<List<InlineKeyboardButton>>();
            var text = string.Empty;
            for(var i = 0; i < page.Items.Count; i++)
            {
                text += $"<b>{i + 1}:</b> " + page.Items[i].PageTitle;
            }

            // Selection buttons
            var firstRow = new List<InlineKeyboardButton>();
            for(var i = 0; i < page.Items.Count; i++)
            {
                firstRow.Add(InlineKeyboardButton.WithCallbackData($"Add {i + 1}", $"{ISearchFormatProvider.SelectData}{i}"));
            }

            if(page.Items.Count != cache.PageSize)
            {
                // Add filler buttons if required
                for(var i = page.Items.Count; i < cache.PageSize; i++)
                {
                    firstRow.Add(InlineKeyboardButton.WithCallbackData($" ", $"{ISearchFormatProvider.DisabledData}"));
                }
            }

            // Prev/Next buttons
            var secondRow = new List<InlineKeyboardButton>();
            if(page.PreviousPossible)
            {
                secondRow.Add(InlineKeyboardButton.WithCallbackData("<<", ISearchFormatProvider.PreviousData));
            }
            else if(page.NextPossible)
            {
                // If next is possible add a spacing button
                secondRow.Add(InlineKeyboardButton.WithCallbackData(ISearchFormatProvider.Spacer, ISearchFormatProvider.DisabledData));
            }

            if(page.NextPossible)
            {
                secondRow.Add(InlineKeyboardButton.WithCallbackData(">>", ISearchFormatProvider.NextData));
            }
            else if(page.PreviousPossible)
            {
                // if previous is possbile add a spacing button
                secondRow.Add(InlineKeyboardButton.WithCallbackData(ISearchFormatProvider.Spacer, ISearchFormatProvider.DisabledData));
            }

            var thirdRow = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(_languageManager.GetTranslation("ButtonCancel"), ISearchFormatProvider.CancelData)
            };

            keyboardMarkupData.Add(firstRow);
            keyboardMarkupData.Add(secondRow);
            keyboardMarkupData.Add(thirdRow);

            var keyboardMarkup = new InlineKeyboardMarkup(keyboardMarkupData);

            var messageText = $"{cache.PageTitle}\n\n{text}";
            _logger.Trace($"Sending pageination ({messageText.Length})");

            if(create)
            {
                _statisticsService.IncreaseMessagesSent();
                var sentMessage = await client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: messageText,
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboardMarkup
                );
                cache.MessageId = sentMessage.MessageId;
                _logger.Trace($"Sent pagination with message id {sentMessage.MessageId}");
            }
            else
            {
                await client.EditMessageTextAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: messageText,
                    parseMode: ParseMode.Html
                );
                await client.EditMessageReplyMarkupAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    replyMarkup: keyboardMarkup
                );
                _logger.Trace($"Updated pagination with message id {message.MessageId}");
            }
        }
    }
}
