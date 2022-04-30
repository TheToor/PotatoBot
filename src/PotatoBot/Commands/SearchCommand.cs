using PotatoBot.Model;
using PotatoBot.Model.Commands;
using PotatoBot.Model.Commands.Data;
using PotatoBot.Model.Commands.FormatProviders;
using PotatoBot.Model.Settings;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("search", Description = "Search for new things to add")]
    public class SearchCommand : ICommand, IReplyCallback, IQueryCallback
    {
        public string UniqueIdentifier => "search";

        private const string DataBoth = "Both";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TelegramService _telegramService;
        private readonly ServiceManager _serviceManager;
        private readonly LanguageService _languageManager;
        private readonly StatisticsService _statisticsService;
        private readonly BotSettings _botSettings;

        public SearchCommand(TelegramService telegramService, ServiceManager serviceManager, LanguageService languageManager, StatisticsService statisticsService, BotSettings botSettings)
        {
            _telegramService = telegramService;
            _serviceManager = serviceManager;
            _languageManager = languageManager;
            _statisticsService = statisticsService;
            _botSettings = botSettings;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var keyboardMarkup = _telegramService.GetDefaultEntertainmentInlineKeyboardButtons();
            if(keyboardMarkup.Count == 0)
            {
                // Nothing enabled so nothing to search for
                await _telegramService.SimpleReplyToMessage(message, _languageManager.GetTranslation("Commands", "Search", "NotEnabled"));
                return true;
            }

            var markup = new InlineKeyboardMarkup(keyboardMarkup);
            var title = _languageManager.GetTranslation("Commands", "Search", "Start");
            await _telegramService.ReplyWithMarkupAndData(
                this,
                message,
                title,
                markup,
                new SearchData(
                    searchFormatProvider:
                        _botSettings.AddPicturesToSearch ?
                        new PictureSearchFormatProvider(_statisticsService, _languageManager) :
                        new ListSearchFormatProvider(_statisticsService, _languageManager)
                )
            );
            return true;
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            var messageData = callbackQuery.Data!;
            var message = callbackQuery.Message!;
            var cacheData = _telegramService.GetCachedData<SearchData>(message);
            if(cacheData == null)
            {
                return false;
            }

            if(cacheData.SelectedSearch == ServarrType.Unknown)
            {
                return await HandleSearchSelection(client, message, messageData, cacheData);
            }
            return await HandleServiceSelection(message, messageData, cacheData);
        }

        private async Task<bool> HandleSearchSelection(TelegramBotClient client, Message message, string messageData, SearchData cacheData)
        {
            if(!Enum.TryParse<ServarrType>(messageData, out var selectedSearch))
            {
                _logger.Warn($"Failed to parse {messageData} to {nameof(ServarrType)}");
                return true;
            }

            cacheData.SelectedSearch = selectedSearch;
            _logger.Trace($"{message.From!.Username} is searching for {selectedSearch}");

            var title = _languageManager.GetTranslation("Commands", "Search", "Selection");
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            foreach(var service in _serviceManager.GetAllServices().Where(s => s is IServarr apiBase && apiBase.Type == selectedSearch))
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(service.Name, service.Name)
                });
            }
            keyboardMarkup.Add(new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(_languageManager.GetTranslation("ButtonBoth"), DataBoth)
            });

            var markup = new InlineKeyboardMarkup(keyboardMarkup);

            await _telegramService.ReplyWithMarkupAndData(this, message, title, markup, cacheData);

            return true;
        }

        private async Task<bool> HandleServiceSelection(Message message, string messageData, SearchData cacheData)
        {
            if(
                _serviceManager.GetAllServices().FirstOrDefault(s =>
                     s is IServarr apiBase &&
                     apiBase.Type == cacheData.SelectedSearch &&
                     apiBase.Name == messageData
                )
                is not IServarr service
            )
            {
                if(messageData != DataBoth)
                {
                    _logger.Warn($"Failed to find service of type {cacheData.SelectedSearch} with name {messageData}");
                    return false;
                }

                cacheData.API = _serviceManager.GetAllServices().Where(
                    s => s is IServarr apiBase &&
                    apiBase.Type == cacheData.SelectedSearch
                ).Cast<IServarr>();
            }
            else
            {
                cacheData.API = new List<IServarr>()
                {
                    service
                };
            }

            _logger.Trace($"{message.Chat.Username} is searching for {cacheData.SelectedSearch} with {cacheData.API.Count()} services");

            var isSingleSearch = cacheData.API.Count() == 1;
            var title = string.Format(
                _languageManager.GetTranslation(
                    "Commands",
                    "Search",
                    isSingleSearch ? "Selected" : "SelectedAll"
                ),
                _languageManager.GetTranslation(cacheData.SelectedSearch.ToString()),
                isSingleSearch ? cacheData.API.First().Name : ""
            );

            await _telegramService.ForceReply(this, message, title);

            return true;
        }

        public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
        {
            var searchText = message.Text!;
            var cacheData = _telegramService.GetCachedData<SearchData>(message);
            if(cacheData == null)
            {
                return false;
            }
            cacheData.SearchText = searchText;

            var searchService = cacheData.API!.First();
            _logger.Trace($"Searching in {searchService.Type} for '{searchText}'");

            await _statisticsService.Increase(TrackedStatistics.Searches);

            await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            cacheData.SearchResults = searchService.Search(searchText);
            var resultCount = cacheData.SearchResults?.Count() ?? 0;
            _logger.Trace($"Found {resultCount} results for '{searchText}'");

            if(resultCount > 0)
            {
                //cacheData.SearchResults = cacheData.SearchResults.OrderByDescending((r) => r.Year);
                var title = string.Format(
                    _languageManager.GetTranslation("Commands", "Search", cacheData.SelectedSearch.ToString()),
                    resultCount
                );

                await _telegramService.ReplyWithPageination(message, title, cacheData.SearchResults!, OnPageinationSelection);
            }
            else
            {
                await _telegramService.SimpleReplyToMessage(message, string.Format(_languageManager.GetTranslation("Commands", "Search", $"No{cacheData.SelectedSearch}"), searchText));
            }

            return true;
        }

        public async Task<bool> OnPageinationSelection(TelegramBotClient client, Message message, int selectedIndex)
        {
            var cacheData = _telegramService.GetCachedData<SearchData>(message);
            var selectedItem = _telegramService.GetPageinationResult(message, selectedIndex);
            if(cacheData == null || selectedItem == null)
            {
                _logger.Warn($"Failed to find pageination result with index {selectedIndex}");
                if(cacheData != null)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, string.Format(_languageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                }
                return true;
            }

            foreach(var service in cacheData.API!)
            {
                var result = service.Add(selectedItem);
                if(result.Added)
                {
                    await _statisticsService.Increase(TrackedStatistics.Adds);
                    await client.SendTextMessageAsync(message.Chat.Id, string.Format(_languageManager.GetTranslation("Commands", "Search", "Success"), selectedItem.Title));
                }
                else if(result.AlreadyAdded)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, string.Format(_languageManager.GetTranslation("Commands", "Search", "Exists"), selectedItem.Title));
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, string.Format(_languageManager.GetTranslation("Commands", "Search", "Fail"), selectedItem.Title));
                }
            }

            return true;
        }
    }
}
