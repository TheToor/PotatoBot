using PotatoBot.Extensions;
using PotatoBot.Model;
using PotatoBot.Model.Commands;
using PotatoBot.Model.Commands.Data;
using PotatoBot.Model.Commands.FormatProviders;
using PotatoBot.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Services
{
    public class TelegramService
    {
        private const ushort MaxMessageLength = 4096;

        public string Name => "Telegram";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BotSettings _settings;
        private readonly LanguageService _languageManager;
        private readonly CommandService _commandManager;
        private readonly StatisticsService _statisticsService;
        private readonly ServiceManager _servarrManager;

        private TelegramBotClient? _client;
        private TelegramBotClient? _alertClient;

        // Thread lock for cache
        private readonly SemaphoreSlim _cacheLock = new (1, 1);
        // Cache to store data "inside" of a telegram chat
        private Dictionary<long, Cache> _cache = new();
        // Timer to invalidate cache
        private System.Timers.Timer? _cacheTimer;
        // Time until a message "expires" in hours
        private readonly uint _cacheInvalidationTime = 24;

        private static bool _isReceiving;
        private readonly CancellationTokenSource _botCancellationTokenSource = new();

        private readonly List<long> _users;
        internal List<long> AllUsers => _users;

        // Characters that need to be escaped with an \
        private readonly string[] _charactersToEscape = new string[]
        {
            /* "*",*/"(", ")", "~", "`", "#", "+", "=", "|", "{", "}"
        };
        // Characters that need to be espcaed with an \ when send in Markdown mode (non-HTML)
        private readonly string[] _charactersToEscapeNonHTML = new string[]
        {
            ">", ".", "[", "]", "-", "!"
        };

        public TelegramService(BotSettings settings, LanguageService languageManager, CommandService commandManager, StatisticsService statisticsService, ServiceManager servarrManager)
        {
            _settings = settings;
            _languageManager = languageManager;
            _commandManager = commandManager;
            _statisticsService = statisticsService;
            _servarrManager = servarrManager;

            _users = _settings.Telegram.Admins.Union(_settings.Telegram.Users).ToList();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Trace("Setting up bot ...");
                _client = new TelegramBotClient(_settings.Telegram.BotToken);

                _logger.Trace("Setting up alert bot ...");
                _alertClient = new TelegramBotClient(_settings.Telegram.AlertBotToken);

                _logger.Trace("Getting bot information ...");
                var botInfo = await _client.GetMeAsync(cancellationToken);
                var alertBotInfo = await _alertClient.GetMeAsync(cancellationToken);
                _logger.Info($"Connected as {botInfo.FirstName} {botInfo.LastName} ({botInfo.Username})");
                _logger.Info($"Alerts connected as {alertBotInfo.FirstName} {alertBotInfo.LastName} ({alertBotInfo.Username})");

                _logger.Trace("Setting up telegram bot ...");
                var commands = _commandManager.Commands
                    .Select((c) => new BotCommand()
                    {
                        Command = c.Name,
                        Description = c.Description
                    });
                await _client.SetMyCommandsAsync(commands);

                _logger.Trace("Initializing cache ...");
                _cacheTimer = new System.Timers.Timer(1000 * 60 * 60)
                {
                    AutoReset = true
                };
                _cacheTimer.Elapsed += ValidateCache;
                _cacheTimer.Start();

                _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions(), _botCancellationTokenSource.Token);
                _isReceiving = true;

                _logger.Info($"Successfully initialized {Name} Service");
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to start {Name} Service");
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _isReceiving = false;
            if(_cacheTimer != null)
            {
                _cacheTimer.Stop();
                _cacheTimer.Dispose();
            }
            _botCancellationTokenSource.Cancel();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private string EscapeMessage(string message, ParseMode parseMode)
        {
            foreach(var character in _charactersToEscape)
            {
                message = message.Replace(character, $"\\{character}");
            }

            if(parseMode != ParseMode.Html)
            {
                foreach(var character in _charactersToEscapeNonHTML)
                {
                    message = message.Replace(character, $"\\{character}");
                }
            }

            return message;
        }

        internal async Task SendSimpleMessage(ChatId chatId, string message, ParseMode parseMode, bool disableNotification = false)
        {
            message = EscapeMessage(message, parseMode);

            _logger.Trace($"Sending '{message}' to {chatId}");

            if(message.Length > MaxMessageLength)
            {
                var messages = SplitMessage(message);

                foreach(var splittedMessage in messages)
                {
                    await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                    await _client!.SendTextMessageAsync(chatId, splittedMessage, parseMode, disableNotification: disableNotification);
                }
            }
            else
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _client!.SendTextMessageAsync(chatId, message, parseMode, disableNotification: disableNotification);
            }
        }

        internal async Task SendSimpleAlertMessage(ChatId chatId, string message, ParseMode parseMode, bool disableNotification = false)
        {
            message = EscapeMessage(message, parseMode);

            _logger.Trace($"Sending '{message}' to {chatId}");

            if(message.Length > MaxMessageLength)
            {
                var messages = SplitMessage(message);

                foreach(var splittedMessage in messages)
                {
                    await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                    await _alertClient!.SendTextMessageAsync(chatId, splittedMessage, parseMode, disableNotification: disableNotification);
                }
            }
            else
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _alertClient!.SendTextMessageAsync(chatId, message, parseMode, disableNotification: disableNotification);
            }
        }

        private static List<string> SplitMessage(string message)
        {
            var messages = new List<string>();

            for(int i = MaxMessageLength; i > 0; i--)
            {
                if(message[i] == '\n')
                {
                    // Split by EOL
                    messages.Add(message.Substring(0, i));
                    // Remove splitted message from message
                    message = message.Substring(i, message.Length - i);
                    // Reset the loop
                    i = message.Length > MaxMessageLength ? MaxMessageLength : message.Length;

                    if(message.Length == 0)
                    {
                        // End loop if we "packed" all messages
                        break;
                    }
                }
            }

            return messages;
        }

        internal async Task SendToAll(string message, bool silent = true)
        {
            foreach(var chat in _users)
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _client!.SendTextMessageAsync(chat, message, ParseMode.Html, disableNotification: silent);
            }
        }

        internal async Task SendToAdmin(string message)
        {
            if(!_isReceiving)
            {
                return;
            }

            message = EscapeMessage(message, ParseMode.Html);

            _logger.Trace($"Sending '{message}' to admins");

            foreach(var chat in _settings.Telegram.Admins)
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _client!.SendTextMessageAsync(chat, message, ParseMode.Html);
            }
        }

        internal bool IsFromAdmin(Message message)
        {
            return _settings.Telegram.Admins.Contains(message.From!.Id);
        }

        private bool IsValidUser(long userId)
        {
            return _users.Contains(userId);
        }

        internal async Task<Message> SimpleReplyToMessage(Message message, string text, ParseMode parseMode = ParseMode.MarkdownV2)
        {
            text = EscapeMessage(text, parseMode);

            _logger.Trace($"Sending '{text}' to {message.From!.Username}");

            if(text.Length > MaxMessageLength)
            {
                var lastMessage = default(Message);
                var messages = SplitMessage(text);
                foreach(var splittedMessage in messages)
                {
                    lastMessage = await _client!.SendTextMessageAsync(message.Chat!, splittedMessage, replyToMessageId: message.MessageId, parseMode: parseMode);
                    await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                }
                return lastMessage!;
            }
            else
            {
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                return await _client!.SendTextMessageAsync(message.Chat!, text, replyToMessageId: message.MessageId, parseMode: parseMode);
            }
        }

        internal async Task<Message> ForceReply(IReplyCallback caller, Message message, string title)
        {
            var cache = GetCache(message);
            cache.ForceReply = true;
            cache.ForceReplyInstance = caller;

            await _statisticsService.Increase(TrackedStatistics.MessagesSent);
            var sentMessage = await _client!.SendTextMessageAsync(message.Chat.Id, title, replyMarkup: new ForceReplyMarkup());
            cache.MessageId = sentMessage.MessageId;

            if(message.From!.IsBot)
            {
                await _client!.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            _logger.Trace($"[{sentMessage.MessageId}] {message.Chat.Username}: {title}");

            return sentMessage;
        }

        internal async Task<Message> ReplyWithMarkupAndData(IQueryCallback caller, Message message, string title, IReplyMarkup markup, IData data)
        {
            if(message.From!.IsBot)
            {
                await _client!.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            var sentMessage = await ReplyWithMarkup(caller, message, title, markup);
            CacheOrUpdate(sentMessage, data);
            return sentMessage;
        }
        internal async Task<Message> ReplyWithMarkup(IQueryCallback caller, Message message, string text, IReplyMarkup markup, ParseMode parseMode = ParseMode.MarkdownV2)
        {
            await _statisticsService.Increase(TrackedStatistics.MessagesSent);
            var sentMessage = await _client!.SendTextMessageAsync(
                chatId: message.Chat!,
                text: EscapeMessage(text, parseMode),
                replyMarkup: markup,
                replyToMessageId: message.From!.IsBot ? 0 : message.MessageId,
                parseMode: parseMode
            );
            var cache = GetCache(sentMessage);
            cache.QueryCallbackInstance = caller;
            return sentMessage;
        }

        internal async Task ReplyWithPageination(
            Message message,
            string title,
            IEnumerable<IServarrItem> list,
            Func<TelegramBotClient, Message, int, Task<bool>> selectionFunction
        )
        {
            _logger.Trace($"Starting pageination for message {message.MessageId} in chat {message.Chat.Id}");

            var cache = GetCache(message);

            _logger.Trace("Preparing page ...");
            cache.PageTitle = title;
            cache.Page = 0;
            if(_settings.AddPicturesToSearch)
            {
                cache.PageSize = 1;
            }
            cache.PageItemList = list;
            cache.PageSelectionFunction = selectionFunction;

            await UpdatePageination(message.ReplyToMessage ?? message, true);
        }

        internal IServarrItem? GetPageinationResult(Message message, int selectedIndex)
        {
            var cache = GetCache(message);

            if(cache.PageItemList == null)
            {
                return default;
            }

            var page = cache.PageItemList.TakePaged(cache.Page, cache.PageSize);
            if(selectedIndex < 0 || selectedIndex >= page.Items.Count)
            {
                return default;
            }

            return page.Items[selectedIndex];
        }

        internal async Task UpdatePageination(Message message, bool create = false)
        {
            _logger.Trace($"Updating pageination of message {message.MessageId}");

            var cache = GetCache(message);
            var page = cache.PageItemList!.TakePaged(cache.Page, cache.PageSize);

            if(cache.Data is not IProvidesSearch)
            {
                throw new InvalidOperationException("Invalid message for pagination");
            }

            await ((IProvidesSearch)cache.Data).SearchFormatProvider.Send(_client!, message, create, cache, page);
        }

        internal T? GetCachedData<T>(Message message) where T : IData
        {
            _cacheLock.Wait();
            try
            {
                if(_cache.ContainsKey(message.Chat.Id))
                {
                    var cache = _cache[message.Chat.Id];
                    cache.LastAccessed = DateTime.Now;
                    return (T)cache.Data!;
                }

                return default;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        internal List<List<InlineKeyboardButton>> GetDefaultEntertainmentInlineKeyboardButtons(bool supportDiscovery = false)
        {
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            var allowedServices = _servarrManager.GetAllServices().Where(s => (s as IServarr) is not null);
            if(supportDiscovery)
            {
                allowedServices = allowedServices.Where(s => (s as IServarrSupportsDiscovery) is not null);
            }

            if(_servarrManager.Radarr?.Count > 0 && _servarrManager.Radarr.Any((s) => allowedServices.Contains(s)))
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Movies", $"{(int)ServarrType.Radarr}")
                });
            }

            if(_servarrManager.Sonarr?.Count > 0 && _servarrManager.Sonarr.Any((s) => allowedServices.Contains(s)))
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Series", $"{(int)ServarrType.Sonarr}")
                });
            }

            if(_servarrManager.Lidarr?.Count > 0 && _servarrManager.Lidarr.Any((s) => allowedServices.Contains(s)))
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Artists", $"{(int)ServarrType.Lidarr}")
                });
            }
            return keyboardMarkup;
        }

        internal Cache GetCache(Message message)
        {
            _logger.Trace($"Getting cache for chat {message.Chat.Id}");
            _cacheLock.Wait();

            try
            {
                if(_cache.ContainsKey(message.Chat.Id))
                {
                    var cache = _cache[message.Chat.Id];
                    cache.LastAccessed = DateTime.Now;
                    return cache;
                }
                else
                {
                    var cache = new Cache()
                    {
                        ChatId = message.Chat.Id,
                        MessageId = message.MessageId,
                    };
                    _cache.Add(message.Chat.Id, cache);
                    return cache;
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private void CacheOrUpdate(Message message, IData data)
        {
            _cacheLock.Wait();

            try
            {
                if(_cache.ContainsKey(message.Chat.Id))
                {
                    var cache = _cache[message.Chat.Id];
                    cache.Data = data;
                    cache.ChatId = message.Chat.Id;
                    cache.MessageId = message.MessageId;

                    cache.LastAccessed = DateTime.Now;
                }
                else
                {
                    _cache.Add(message.Chat.Id, new Cache
                    {
                        Data = data,
                        ChatId = message.Chat.Id,
                        MessageId = message.MessageId
                    });
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private void ValidateCache(object? sender, System.Timers.ElapsedEventArgs? e)
        {
            _logger.Trace("Validating cache ...");

            _cacheLock.Wait();

            try
            {
                var newCachedRequired = false;
                var newCache = new Dictionary<long, Cache>();
                foreach(var cacheItem in _cache)
                {
                    var key = cacheItem.Key;
                    var value = cacheItem.Value;

                    if(value.LastAccessed.AddHours(_cacheInvalidationTime) < DateTime.Now)
                    {
                        // Item is expired
                        newCachedRequired = true;
                        // Delete message since the data is gone so it shouldn't be used anymore
                        if(!(value.ChatId == null))
                        {
                            _client!.DeleteMessageAsync(value.ChatId, value.MessageId);
                        }
                        continue;
                    }

                    newCache.Add(key, value);
                }

                if(newCachedRequired)
                {
                    _cache = newCache;
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                _logger.Trace($"Received message of type {update.Type}");
                var handler = update.Type switch
                {
                    UpdateType.Message => OnNewMessage(update.Message!),
                    UpdateType.CallbackQuery => OnCallbackQueryReceived(update.CallbackQuery!),
                    _ => HandleUnknownUpdate(update)
                };

                try
                {
                    await handler;
                }
                catch(Exception ex)
                {
                    await HandleErrorAsync(botClient, ex, cancellationToken);
                }
            }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Warn($"Error in TelegramManager: {exception}");
            return;
        }

        private static async Task HandleUnknownUpdate(Update update)
        {
            _logger.Debug($"Received unknown message of type {update.Type}");
            return;
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private async Task OnNewMessage(Message message)
        {
            try
            {
                var user = message.From;

                _logger.Trace($"Received new message from [{user!.Id}] {user.Username}: {message.Text}");

                // Discard messages from bots
                if(message.From!.IsBot)
                {
                    _logger.Trace("Discarding bot message");
                    return;
                }

                // Discard forwarded messages
                if(message.ForwardFrom != null || message.ForwardFromChat != null)
                {
                    _logger.Trace("Discarding forwarded message");
                    return;
                }

                // Discard non text messages
                if(message.Type != MessageType.Text || string.IsNullOrEmpty(message.Text))
                {
                    _logger.Trace($"Discarding non-text message ({message.Type})");
                    return;
                }

                // Discard message from non users
                if(!IsValidUser(user.Id))
                {
                    _logger.Trace("Discarding messafe from non whitelisted sender");
                    await SimpleReplyToMessage(message, $"<b>Not Registered</b>.\nPlease ask an administrator to whitelist your id: '{user.Id}'");
                    return;
                }

                if(message.Text.StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Command
                    await _statisticsService.Increase(TrackedStatistics.CommandsReceived);

                    _logger.Trace("Detected command");
                    await _commandManager!.ProcessCommandMessage(_client!, message);
                }
                else
                {
                    await _statisticsService.Increase(TrackedStatistics.MessagesReceived);

                    // Reply ?
                    _logger.Trace("Detected message");

                    // Force reply possible
                    if(message.ReplyToMessage != null)
                    {
                        var replyToMessageId = message.ReplyToMessage.MessageId;
                        _logger.Trace($"Message {message.MessageId} is a reply to {replyToMessageId}. Checking for Force Reply ...");

                        await _cacheLock.WaitAsync();
                        try
                        {
                            if(_cache.ContainsKey(message.Chat.Id))
                            {
                                _cacheLock.Release();
                                var cache = GetCache(message.ReplyToMessage);

                                if(cache.MessageId == replyToMessageId)
                                {
                                    _logger.Trace($"Detected {replyToMessageId} as reply. Invoking action ...");
                                    // Reset ForceReply flag
                                    cache.ForceReply = false;
                                    // Invoke Event Handler
                                    await cache.ForceReplyInstance!.OnReplyReceived(_client!, message);
                                }
                            }
                            else
                            {
                                _logger.Trace($"Not awaiting a reply to {replyToMessageId}");
                            }
                        }
                        finally
                        {
                            if(_cacheLock.CurrentCount == 0)
                            {
                                _cacheLock.Release();
                            }
                        }
                    }
                    else
                    {
                        _logger.Trace("Message is not a reply. Stopping further processing");
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to process message");
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _client!.SendTextMessageAsync(message.Chat.Id, _languageManager.GetTranslation("GeneralError"), replyToMessageId: message.MessageId);
            }
        }

        private async Task OnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            try
            {
                _logger.Trace($"Received CallbackQuery from {callbackQuery.From.Username}");

                var data = callbackQuery.Data;
                var message = callbackQuery.Message!;

                await _cacheLock.WaitAsync();

                try
                { 
                    if(_cache.ContainsKey(message.Chat.Id))
                    {
                        var cache = _cache[message.Chat.Id];
                        _cacheLock.Release();

                        if(cache.Data is IProvidesSearch searchData && await searchData.SearchFormatProvider!.HandlePagination(this, _client!, message, cache, data!))
                        {
                            return;
                        }

                        if(!await cache.QueryCallbackInstance!.OnCallbackQueryReceived(_client!, callbackQuery))
                        {
                            _logger.Warn($"Failed to execute callback for '{cache.QueryCallbackInstance}'");
                        }
                    }
                    else
                    {
                        _logger.Debug($"No callback found for message {message.MessageId}");
                        await _client!.DeleteMessageAsync(message.Chat!, message.MessageId);
                        _cacheLock.Release();
                    }
                }
                catch
                {
                    if(_cacheLock.CurrentCount == 0)
                    {
                        _cacheLock.Release();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to process CallbackQuery");
                await _statisticsService.Increase(TrackedStatistics.MessagesSent);
                await _client!.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, _languageManager.GetTranslation("GeneralError"));
            }
        }
    }
}
