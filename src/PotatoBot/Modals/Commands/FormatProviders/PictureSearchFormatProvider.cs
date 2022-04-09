using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Modals.Commands.FormatProviders
{
    public class PictureSearchFormatProvider : ISearchFormatProvider
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly StatisticsService _statisticsService;
        private readonly LanguageService _languageManager;
        public PictureSearchFormatProvider(StatisticsService statisticsService, LanguageService languageManager)
        {
            _statisticsService = statisticsService;
            _languageManager = languageManager;
        }

        public async Task Send(TelegramBotClient client, Message message, bool create, Cache cache, PageResult<IServarrItem> page)
        {
            var keyboardMarkupData = new List<List<InlineKeyboardButton>>();
            var text = page.Items[0].PageTitle;

            // Selection buttons
            var firstRow = new List<InlineKeyboardButton>();
            var secondRow = new List<InlineKeyboardButton>();

            // Prev button
            if(page.PreviousPossible)
            {
                firstRow.Add(InlineKeyboardButton.WithCallbackData(_languageManager.GetTranslation("ButtonPrev"), ISearchFormatProvider.PreviousData));
                secondRow.Add(InlineKeyboardButton.WithCallbackData($"{_languageManager.GetTranslation("ButtonPrev")} 5", ISearchFormatProvider.PreviousFiveData));
            }
            else if(page.NextPossible)
            {
                // If next is possible add a spacing button
                firstRow.Add(InlineKeyboardButton.WithCallbackData(ISearchFormatProvider.Spacer, ISearchFormatProvider.DisabledData));
                secondRow.Add(InlineKeyboardButton.WithCallbackData(ISearchFormatProvider.Spacer, ISearchFormatProvider.DisabledData));
            }

            // Middle add button
            firstRow.Add(InlineKeyboardButton.WithCallbackData(_languageManager.GetTranslation("ButtonAdd"), $"{ISearchFormatProvider.SelectData}{0}"));

            // Next button
            if(page.NextPossible)
            {
                firstRow.Add(InlineKeyboardButton.WithCallbackData(_languageManager.GetTranslation("ButtonNext"), ISearchFormatProvider.NextData));
                secondRow.Add(InlineKeyboardButton.WithCallbackData($"{_languageManager.GetTranslation("ButtonNext")} 5", ISearchFormatProvider.NextFiveData));
            }
            else if(page.PreviousPossible)
            {
                // if previous is possbile add a spacing button
                firstRow.Add(InlineKeyboardButton.WithCallbackData(ISearchFormatProvider.Spacer, ISearchFormatProvider.DisabledData));
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
            if(messageText.Length >= 200)
            {
                messageText = $"{messageText.Substring(0, 190)} ...";
            }

            _logger.Trace($"Sending pageination ({messageText.Length})");

            var posterUrl = page.Items[0].GetPosterUrl();
            if(create)
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);

                Message sentMessage;
                if(string.IsNullOrEmpty(posterUrl))
                {
                    sentMessage = await client.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: ISearchFormatProvider.MissingImageUrl,
                        parseMode: ParseMode.Html,
                        caption: messageText,
                        replyMarkup: keyboardMarkup
                    );
                }
                else
                {
                    try
                    {
                        sentMessage = await client.SendPhotoAsync(
                            chatId: message.Chat.Id,
                            photo: page.Items[0].GetPosterUrl(),
                            parseMode: ParseMode.Html,
                            caption: messageText,
                            replyMarkup: keyboardMarkup
                        );
                    }
                    catch(Exception)
                    {
                        sentMessage = await client.SendPhotoAsync(
                            chatId: message.Chat.Id,
                            photo: ISearchFormatProvider.MissingImageUrl,
                            parseMode: ParseMode.Html,
                            caption: messageText,
                            replyMarkup: keyboardMarkup
                        );
                    }
                }

                cache.MessageId = sentMessage.MessageId;
                _logger.Trace($"Sent pagination with message id {sentMessage.MessageId}");
            }
            else
            {
                try
                {
                    await client.EditMessageMediaAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        media: new InputMediaPhoto(new InputMedia(posterUrl ?? ISearchFormatProvider.MissingImageUrl))
                    );
                }
                catch(Exception)
                {
                    await client.EditMessageMediaAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId,
                        media: new InputMediaPhoto(new InputMedia(ISearchFormatProvider.MissingImageUrl))
                    );
                }

                await client.EditMessageCaptionAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: messageText,
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
