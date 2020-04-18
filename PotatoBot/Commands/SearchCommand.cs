using PotatoBot.Managers;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Sonarr;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
using System;
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
            cacheData.SearchText = searchText;

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
                        cacheData.MovieSearchResults = RadarrService.SearchMovieByName(searchText);

                        var resultCount = cacheData.MovieSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} movies for '{searchText}'");

                        if(resultCount > 0)
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
                        cacheData.ArtistSearchResults = LidarrService.SearchAristsByName(searchText);

                        var resultCount = cacheData.ArtistSearchResults?.Count ?? 0;

                        _logger.Trace($"Found {resultCount} artists for '{searchText}'");

                        if(resultCount > 0)
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

            switch(cacheData.SelectedSearch)
            {
                case SearchType.Series:
                    {
                        var selectedSeries = TelegramService.GetPageinationResult<Series>(message, selectedIndex);
                        if(selectedSeries == null)
                        {
                            _logger.Trace($"Failed to find pageination result with index {selectedIndex}");
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                            return true;
                        }

                        var result = SonarrService.AddSeries(selectedSeries);
                        if(result.Added)
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(Program.LanguageManager.GetTranslation("Commands", "Search", "Success"), selectedSeries.Title));
                        }
                        else if(result.AlreadyAdded)
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
                        if(selectedMovie == null)
                        {
                            _logger.Trace($"Failed to find pageination result with index {selectedIndex}");
                            await client.SendTextMessageAsync(message.Chat.Id, string.Format(LanguageManager.GetTranslation("Commands", "Search", "Fail"), cacheData.SearchText));
                            return true;
                        }

                        var result = RadarrService.AddMovie(selectedMovie);
                        if (result.Added)
                        {
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

                        var result = LidarrService.AddArtist(selectedArtist);
                        if (result.Added)
                        {
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
