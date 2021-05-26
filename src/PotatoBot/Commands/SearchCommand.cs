using Microsoft.AspNetCore.Routing;
using PotatoBot.API;
using PotatoBot.Managers;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Radarr;
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
            var service = default(APIBase);
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
            _logger.Trace($"{message.From.Username} is searching for {cacheData.SelectedSearch} with {service.Name}");

            var title = string.Format(
                Program.LanguageManager.GetTranslation("Commands", "Search", "Selected"),
                cacheData.SelectedSearch.ToString(),
                service.Name
            );

            await TelegramService.ForceReply(this, message, title);

            return true;
        }

        public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
        {
            var searchText = message.Text;
            var cacheData = TelegramService.GetCachedData<SearchData>(message);
            cacheData.SearchText = searchText;

            _logger.Trace($"Searching in {cacheData.API.Name} for '{searchText}'");

            StatisticsService.IncreaseSearches();

            await client.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            switch (cacheData.SelectedSearch)
            {
                case SearchType.Series:
                    {
                        cacheData.SeriesSearchResults = (cacheData.API as SonarrService).SearchSeries(searchText);

                        var resultCount = cacheData.SeriesSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} series for '{searchText}'");

                        if (resultCount > 0)
                        {
                            cacheData.SeriesSearchResults = cacheData.SeriesSearchResults.OrderByDescending((s) => s.Year).ToList();

                            var title = string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Series"), cacheData.SeriesSearchResults.Count);

                            var formatFunction = new Func<object, string>((obj) =>
                            {
                                var series = obj as Series;
                                return $"<b>{series.Year} - {series.Title}</b>\n{series.Overview}\n\n";
                            });

                            await TelegramService.ReplyWithPageination(message, title, cacheData.SeriesSearchResults, formatFunction, OnPageinationSelection);
                        }
                        else
                        {
                            await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", "NoSeries"), searchText));
                        }
                    }
                    break;

                case SearchType.Movie:
                    {
                        cacheData.MovieSearchResults = (cacheData.API as RadarrService).SearchMovieByName(searchText);

                        var resultCount = cacheData.MovieSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} movies for '{searchText}'");

                        if (resultCount > 0)
                        {
                            cacheData.MovieSearchResults = cacheData.MovieSearchResults.OrderByDescending((m) => m.Year).ToList();

                            var title = string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Movie"), cacheData.MovieSearchResults.Count);

                            var formatFunction = new Func<object, string>((obj) =>
                            {
                                var movie = obj as Movie;
                                return $"<b>{movie.Year} - {movie.Title}</b>\n{movie.Overview}\n\n";
                            });

                            await TelegramService.ReplyWithPageination(message, title, cacheData.MovieSearchResults, formatFunction, OnPageinationSelection);
                        }
                        else
                        {
                            await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", "NoMovie"), searchText));
                        }
                    }
                    break;

                case SearchType.Artist:
                    {
                        cacheData.ArtistSearchResults = (cacheData.API as LidarrService).SearchAristsByName(searchText);

                        var resultCount = cacheData.ArtistSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} artists for '{searchText}'");

                        if (resultCount > 0)
                        {
                            var title = string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Artist"), cacheData.ArtistSearchResults.Count);

                            var formatFunction = new Func<object, string>((obj) =>
                            {
                                var artist = obj as Artist;
                                return $"<b>{artist.ArtistName}</b>\n{artist.Overview}\n\n";
                            });

                            await TelegramService.ReplyWithPageination(message, title, cacheData.ArtistSearchResults, formatFunction, OnPageinationSelection);
                        }
                        else
                        {
                            await TelegramService.SimpleReplyToMessage(message, string.Format(LanguageManager.GetTranslation("Commands", "Search", "NoArtist"), searchText));
                        }
                    }
                    break;
            }

            return true;
        }

        public async Task<bool> OnPageinationSelection(TelegramBotClient client, Message message, int selectedIndex)
        {
            var cacheData = TelegramService.GetCachedData<SearchData>(message);

            switch (cacheData.SelectedSearch)
            {
                case SearchType.Series:
                    {
                        var selectedSeries = TelegramService.GetPageinationResult<Series>(message, selectedIndex);
                        if (selectedSeries == null)
                        {
                            _logger.Trace($"Failed to find pageination result with index {selectedIndex}");
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                            return true;
                        }

                        var result = (cacheData.API as SonarrService).AddSeries(selectedSeries);
                        if (result.Added)
                        {
                            StatisticsService.IncreaseAdds();
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedSeries.Title));
                        }
                        else if (result.AlreadyAdded)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Exists"), selectedSeries.Title));
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Fail"), selectedSeries.Title));
                        }
                    }
                    break;

                case SearchType.Movie:
                    {
                        var selectedMovie = TelegramService.GetPageinationResult<Movie>(message, selectedIndex);
                        if (selectedMovie == null)
                        {
                            _logger.Trace($"Failed to find pageination result with index {selectedIndex}");
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                            return true;
                        }

                        var result = (cacheData.API as RadarrService).AddMovie(selectedMovie);
                        if (result.Added)
                        {
                            StatisticsService.IncreaseAdds();
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedMovie.Title));
                        }
                        else if (result.AlreadyAdded)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Exists"), selectedMovie.Title));
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Fail"), selectedMovie.Title));
                        }
                    }
                    break;

                case SearchType.Artist:
                    {
                        var selectedArtist = TelegramService.GetPageinationResult<Artist>(message, selectedIndex);
                        if (selectedArtist == null)
                        {
                            _logger.Trace($"Failed to find pageination result with index {selectedIndex}");
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                            return true;
                        }

                        var result = (cacheData.API as LidarrService).AddArtist(selectedArtist);
                        if (result.Added)
                        {
                            StatisticsService.IncreaseAdds();
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedArtist.ArtistName));
                        }
                        else if (result.AlreadyAdded)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Exists"), selectedArtist.ArtistName));
                        }
                        else
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Fail"), selectedArtist.ArtistName));
                        }
                    }
                    break;
            }

            return true;
        }
    }
}
