using Microsoft.AspNetCore.Routing;
using PotatoBot.API;
using PotatoBot.Managers;
using PotatoBot.Modals;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("search", Description = "Searched for new things to add")]
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
                await TelegramService.SimpleReplyToMessage(message, LanguageManager.GetTranslation("Commands", "Search", "NotEnabled"));
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

            if(cacheData.SelectedSearch == SearchType.None)
            {
                return await HandleSearchSelection(client, message, messageData, cacheData);
            }
            return await HandleServiceSelection(client, message, messageData, cacheData);
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

            var title = Program.LanguageManager.GetTranslation("Commands", "Search", "Selection");
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            switch (selectedSearch)
            {
                case SearchType.Movie:
                    {
                        foreach(var radarr in Program.ServiceManager.Radarr)
                        {
                            keyboardMarkup.Add(new List<InlineKeyboardButton>()
                            {
                                InlineKeyboardButton.WithCallbackData(radarr.Name, radarr.Name)
                            });
                        }
                    }
                    break;

                case SearchType.Series:
                    {
                        foreach (var sonarr in Program.ServiceManager.Sonarr)
                        {
                            keyboardMarkup.Add(new List<InlineKeyboardButton>()
                            {
                                InlineKeyboardButton.WithCallbackData(sonarr.Name, sonarr.Name)
                            });
                        }
                    }
                    break;

                case SearchType.Artist:
                    {
                        foreach (var lidarr in Program.ServiceManager.Lidarr)
                        {
                            keyboardMarkup.Add(new List<InlineKeyboardButton>()
                            {
                                InlineKeyboardButton.WithCallbackData(lidarr.Name, lidarr.Name)
                            });
                        }
                    }
                    break;
            }

            var markup = new InlineKeyboardMarkup(keyboardMarkup);

            await TelegramService.ReplyWithMarkupAndData(this, message, title, markup, cacheData);

            return true;
        }

        private async Task<bool> HandleServiceSelection(TelegramBotClient client, Message message, string messageData, SearchData cacheData)
        {
            var service = default(IServarr);
            switch(cacheData.SelectedSearch)
            {
                case SearchType.Movie:
                    service = RadarrService.First(r => r.Name == messageData);
                    break;
                case SearchType.Series:
                    service = SonarrService.First(s => s.Name == messageData);
                    break;
                case SearchType.Artist:
                    service = LidarrService.First(l => l.Name == messageData);
                    break;
            }

            cacheData.API = service;
            _logger.Trace($"{message.From.Username} is searching for {cacheData.SelectedSearch} with {service.Type}");

            var title = string.Format(
                Program.LanguageManager.GetTranslation("Commands", "Search", "Selected"),
                cacheData.SelectedSearch.ToString(),
                (service as IService).Name
            );

            await TelegramService.ForceReply(this, message, title);

            return true;
        }

        public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
        {
            var searchText = message.Text;
            var cacheData = TelegramService.GetCachedData<SearchData>(message);
            cacheData.SearchText = searchText;

            _logger.Trace($"Searching in {cacheData.API.Type} for '{searchText}'");

            StatisticsService.IncreaseSearches();

            await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            cacheData.SearchResults = cacheData.API.Search(searchText);
            var resultCount = cacheData.SearchResults?.Count() ?? 0;
            _logger.Trace($"Found {resultCount} results for '{searchText}'");

            if (resultCount > 0)
            {
                cacheData.SearchResults = cacheData.SearchResults.OrderByDescending((r) => r.Year);
                var title = string.Format(
                    Program.LanguageManager.GetTranslation("Commands", "Search", cacheData.SelectedSearch.ToString()),
                    resultCount
                );

                await TelegramService.ReplyWithPageination(message, title, cacheData.SearchResults, OnPageinationSelection);
            }
            else
            {
                await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", $"No{cacheData.SelectedSearch}"), searchText));
            }

            return true;
        }

        public async Task<bool> OnPageinationSelection(TelegramBotClient client, Message message, int selectedIndex)
        {
            var cacheData = TelegramService.GetCachedData<SearchData>(message);

            var selectedItem = TelegramService.GetPageinationResult(message, selectedIndex);
            if(selectedItem == null)
            {
                _logger.Warn($"Failed to find pageination result with index {selectedIndex}");
                await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                return true;
            }

            var result = cacheData.API.Add(selectedItem);
            if (result.Added)
            {
                StatisticsService.IncreaseAdds();
                await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedItem.Title));
            }
            else if (result.AlreadyAdded)
            {
                await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Exists"), selectedItem.Title));
            }
            else
            {
                await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Fail"), selectedItem.Title));
            }

            return true;
        }
    }
}
