using PotatoBot.Managers;
using PotatoBot.Modals.API.Sonarr;
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
    [Command("search")]
    internal class SearchCommand : Service, ICommand, IReplyCallback, IQueryCallback
    {
        public string UniqueIdentifier => "search";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var keyboardMarkup = TelegramService.GetDefaultEntertainmentInlineKeyboardButtons();
            if(keyboardMarkup.Count == 0)
            {
                // Nothing enabled so nothing to search for
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Queue", "NotEnabled"));
                return true;
            }

            var markup = new InlineKeyboardMarkup(keyboardMarkup);
            var title = LanguageManager.GetTranslation("Commands", "Search", "Start");
            await TelegramService.ReplyWithMarkupAndData(this, message, title, markup, new SearchData()
            {
                SelectedSearch = SearchType.None
            });
            return true;
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQueryEventArgs e)
        {
            var messageData = e.CallbackQuery.Data;
            var message = e.CallbackQuery.Message;
            var cacheData = TelegramService.GetCachedData<SearchData>(message);

            switch(cacheData.SelectedSearch)
            {
                case SearchType.None:
                    return await HandleSearchSelection(client, message, messageData, cacheData);
            }

            _logger.Warn($"Unhandled SearchType: {cacheData.SelectedSearch}");
            return true;
        }

        private async Task<bool> HandleSearchSelection(TelegramBotClient client, Message message, string messageData, SearchData cacheData)
        {
            if(!Enum.TryParse<SearchType>(messageData, out var selectedSearch))
            {
                _logger.Warn($"Failed to parase {messageData} to {nameof(SearchType)}");
                return true;
            }

            cacheData.SelectedSearch = selectedSearch;
            _logger.Trace($"{message.From.Username} is searching for {selectedSearch}");

            var title = string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Selected"), selectedSearch.ToString());

            await TelegramService.ForceReply(this, message, title);

            return true;
        }

        public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
        {
            var searchText = message.Text;
            var cacheData = TelegramService.GetCachedData<SearchData>(message);

            _logger.Trace($"Searching in {cacheData.SelectedSearch} for '{searchText}'");

            await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            switch(cacheData.SelectedSearch)
            {
                case SearchType.Series:
                    {
                        cacheData.SeriesSearchResults = SonarrService.SearchSeries(searchText);

                        var resultCount = cacheData.SeriesSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} series for '{searchText}'");

                        if (resultCount > 0)
                        {
                            cacheData.SeriesSearchResults = cacheData.SeriesSearchResults.OrderByDescending((s) => s.Year).ToList();

                            var title = string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Series"), cacheData.SeriesSearchResults.Count);

                            var formatFunction = new Func<object, string>((obj) =>
                            {
                                var series = obj as Series;
                                return $"<b>{series.Year}: {series.Title}</b>\n{series.Overview}\n\n";
                            });

                            await TelegramService.ReplyWithPageination(message, title, cacheData.SeriesSearchResults, formatFunction);
                        }
                        else
                        {
                            await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", "NoSeries"), searchText));
                        }
                    }
                    break;
            }

            return true;
        }
    }
}
