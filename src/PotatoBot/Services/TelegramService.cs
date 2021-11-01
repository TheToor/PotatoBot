using PotatoBot.Extensions;
using PotatoBot.Managers;
using PotatoBot.Modals;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Services
{
    internal class TelegramService : IService
    {
        const string PreviousData = "Previous";
        const string NextData = "Next";
        const string SelectData = "Select";
        const string DisabledData = "Disabled";
        const string CancelData = "Cancel";

        private const ushort _maxMessageLength = 4096;

        public string Name => "Telegram";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static TelegramSettings _settings => Program.Settings.Telegram;

        private TelegramBotClient _client;

        private CommandManager _commandManager;

        // Thread lock for cache
        private readonly object _cacheLock = new();
        // Cache to store data "inside" of a telegram chat
        private Dictionary<long, Cache> _cache = new();
        // Timer to invalidate cache
        private System.Timers.Timer _cacheTimer;
        // Time until a message "expires" in hours
        private readonly uint _cacheInvalidationTime = 24;

        private static bool _isReceiving;
        private readonly CancellationTokenSource _botCancellationTokenSource = new();

        private readonly List<long> _users;

        // Characters that need to be escaped with an \
        readonly string[] _charactersToEscape = new string[]
        {
            "_", /* "*",*/ "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!"
        };

        internal TelegramService()
        {
            _users = _settings.Admins.Union(_settings.Users).ToList();
        }

        public bool Start()
        {
            try
            {
                _logger.Trace("Setting up bot ...");
                _client = new TelegramBotClient(_settings.BotToken);

                _logger.Trace("Getting bot information ...");
                var info = _client.GetMeAsync().Result;
                _logger.Info($"Connected as {info.FirstName} {info.LastName} ({info.Username})");

                _logger.Trace("Initializing command manager ...");
                _commandManager = new CommandManager();

                _logger.Trace("Setting up telegram bot ...");
                var commands = _commandManager.Commands
                    .Select((c) => new BotCommand()
                    {
                        Command = c.Name,
                        Description = c.Description
                    });
                _client.SetMyCommandsAsync(commands);

                _logger.Trace("Initializing cache ...");
                _cacheTimer = new System.Timers.Timer(1000 * 60 * 60)
                {
                    AutoReset = true
                };
                _cacheTimer.Elapsed += ValidateCache;
                _cacheTimer.Start();

                // Add additional delay before we start receiving
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(5000);

                    _logger.Trace("Starting ...");
                    _client.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), _botCancellationTokenSource.Token);
                    _isReceiving = true;
                });
                
                _logger.Info($"Successfully initialized {Name} Service");
                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to start {Name} Service");
                return false;
            }
        }

        public bool Stop()
        {
            _isReceiving = false;
            _cacheTimer.Stop();
            _cacheTimer.Dispose();
            _botCancellationTokenSource.Cancel();
            return true;
        }

        internal async Task SendSimpleMessage(ChatId chatId, string message, ParseMode parseMode, bool disableNotification = false)
        {
            if(message.Length > _maxMessageLength)
            {
                var messages = SplitMessage(message);

                foreach (var splittedMessage in messages)
                {
                    await _client.SendTextMessageAsync(chatId, splittedMessage, parseMode, disableNotification: disableNotification);
                }
            }
            else
            {
                await _client.SendTextMessageAsync(chatId, message, parseMode, disableNotification: disableNotification);
            }
        }

        private static List<string> SplitMessage(string message)
        {
            var messages = new List<string>();

            for (int i = _maxMessageLength; i > 0; i--)
            {
                if (message[i] == '\n')
                {
                    // Split by EOL
                    messages.Add(message.Substring(0, i));
                    // Remove splitted message from message
                    message = message.Substring(i, message.Length - i);
                    // Reset the loop
                    i = message.Length > _maxMessageLength ? _maxMessageLength : message.Length;

                    if (message.Length == 0)
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
            foreach (var chat in _users)
            {
                Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
                await _client.SendTextMessageAsync(chat, message, ParseMode.Html, disableNotification: silent);
            }
        }

        internal async Task SendToAdmin(string message)
        {
            if(!_isReceiving)
            {
                return;
            }

            foreach(var character in _charactersToEscape)
            {
                message = message.Replace(character, $"\\{character}");
            }

            _logger.Trace($"Sending '{message}' to admins");

            foreach (var chat in _settings.Admins)
            {
                Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
                await _client.SendTextMessageAsync(chat, message, ParseMode.MarkdownV2);
            }
        }

        internal static bool IsFromAdmin(Message message)
        {
            return _settings.Admins.Contains(message.From.Id);
        }

        internal async Task<Message> SimpleReplyToMessage(Message message, string text, ParseMode parseMode = ParseMode.Default)
        {
            _logger.Trace($"Sending '{text}' to {message.From.Username}");

            Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
            return await _client.SendTextMessageAsync(message.Chat, text, replyToMessageId: message.MessageId, parseMode: parseMode);
        }

        internal async Task<Message> ForceReply(IReplyCallback caller, Message message, string title)
        {
            var cache = GetCache(message);
            cache.ForceReply = true;
            cache.ForceReplyInstance = caller;

            Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
            var sentMessage = await _client.SendTextMessageAsync(message.Chat.Id, title, replyMarkup: new ForceReplyMarkup());
            cache.MessageId = sentMessage.MessageId;

            if(message.From.IsBot)
            {
                await _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            _logger.Trace($"[{sentMessage.MessageId}] {message.Chat.Username}: {title}");

            return sentMessage;
        }

        internal async Task<Message> ReplyWithMarkupAndData(IQueryCallback caller, Message message, string title, IReplyMarkup markup, IData data)
        {
            if (message.From.IsBot)
            {
                await _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }

            var sentMessage = await ReplyWithMarkup(caller, message, title, markup);
            CacheOrUpdate(sentMessage, data);
            return sentMessage;
        }
        internal async Task<Message> ReplyWithMarkup(IQueryCallback caller, Message message, string title, IReplyMarkup markup)
        {
            Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
            var sentMessage = await _client.SendTextMessageAsync(message.Chat, title, replyMarkup: markup, replyToMessageId: (message.From.IsBot ? 0 : message.MessageId));
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
            cache.PageItemList = list;
            cache.PageSelectionFunction = selectionFunction;

            await UpdatePageination(message.ReplyToMessage, true);
        }

        internal IServarrItem GetPageinationResult(Message message, int selectedIndex)
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
            var text = string.Empty;

            var page = cache.PageItemList.TakePaged(cache.Page, cache.PageSize);
            for(var i = 0; i < page.Items.Count; i++)
            {
                text += $"<b>{i + 1}:</b> " + page.Items[i].PageTitle;
            }

            _logger.Trace("Building button layout ...");

            // Selection buttons
            var firstRow = new List<InlineKeyboardButton>();
            for (var i = 0; i < page.Items.Count; i++)
            {
                firstRow.Add(InlineKeyboardButton.WithCallbackData($"{i + 1}", $"{SelectData}{i}"));
            }

            if (page.Items.Count != cache.PageSize)
            {
                // Add filler buttons if required
                for (var i = page.Items.Count; i < cache.PageSize; i++)
                {
                    firstRow.Add(InlineKeyboardButton.WithCallbackData($" ", $"{DisabledData}"));
                }
            }

            // Prev/Next buttons
            var secondRow = new List<InlineKeyboardButton>();
            if (page.PreviousPossible)
            {
                secondRow.Add(InlineKeyboardButton.WithCallbackData("<<", PreviousData));
            }
            else if(page.NextPossible)
            {
                // If next is possible add a spacing button
                secondRow.Add(InlineKeyboardButton.WithCallbackData("  ", DisabledData));
            }

            if (page.NextPossible)
            {
                secondRow.Add(InlineKeyboardButton.WithCallbackData(">>", NextData));
            }
            else if(page.PreviousPossible)
            {
                // if previous is possbile add a spacing button
                secondRow.Add(InlineKeyboardButton.WithCallbackData("  ", DisabledData));
            }

            var thirdRow = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("Cancel", CancelData)
            };

            var keyboardMarkup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                firstRow,
                secondRow,
                thirdRow
            });

            var messageText = $"{cache.PageTitle}\n\n{text}";
            _logger.Trace($"Sending pageination ({messageText.Length})");

            if (create)
            {
                Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
                var sentMessage = await _client.SendTextMessageAsync(message.Chat.Id, messageText, parseMode: ParseMode.Html, replyMarkup: keyboardMarkup);
                cache.MessageId = sentMessage.MessageId;
                _logger.Trace($"Sent pagination with message id {sentMessage.MessageId}");
            }
            else
            {
                await _client.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, parseMode: ParseMode.Html);
                await _client.EditMessageReplyMarkupAsync(message.Chat.Id, message.MessageId, keyboardMarkup);
                _logger.Trace($"Updated pagination with message id {message.MessageId}");
            }
        }

        internal T GetCachedData<T>(Message message) where T : IData
        {
            lock(_cacheLock)
            {
                if(_cache.ContainsKey(message.Chat.Id))
                {
                    var cache = _cache[message.Chat.Id];
                    cache.LastAccessed = DateTime.Now;
                    return (T)cache.Data;
                }

                return default;
            }
        }

        internal static List<List<InlineKeyboardButton>> GetDefaultEntertainmentInlineKeyboardButtons()
        {
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            if (Program.ServiceManager.Radarr?.Count > 0)
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Movies", $"{(int)ServarrType.Radarr}")
                });
            }

            if (Program.ServiceManager.Sonarr?.Count > 0)
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Series", $"{(int)ServarrType.Sonarr}")
                });
            }

            if (Program.ServiceManager.Lidarr?.Count > 0)
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
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(message.Chat.Id))
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
        }

        private void CacheOrUpdate(Message message, IData data)
        {
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(message.Chat.Id))
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
        }

        private void ValidateCache(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.Trace("Validating cache ...");

            lock (_cacheLock)
            {
                var newCachedRequired = false;
                var newCache = new Dictionary<long, Cache>();
                foreach (var cacheItem in _cache)
                {
                    var key = cacheItem.Key;
                    var value = cacheItem.Value;

                    if (value.LastAccessed.AddHours(_cacheInvalidationTime) < DateTime.Now)
                    {
                        // Item is expired
                        newCachedRequired = true;
                        // Delete message since the data is gone so it shouldn't be used anymore
                        _client.DeleteMessageAsync(value.ChatId, value.MessageId);
                        continue;
                    }

                    newCache.Add(key, value);
                }

                if (newCachedRequired)
                {
                    _cache = newCache;
                }
            }
        }

        private static bool IsValidUser(long userId)
        {
            if(_settings.Users.Contains(userId) || _settings.Admins.Contains(userId))
            {
                return true;
            }
            return false;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnNewMessage(update.Message),
                UpdateType.CallbackQuery => OnCallbackQueryReceived(update.CallbackQuery),
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
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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

                _logger.Trace($"Received new message from [{user.Id}] {user.Username}: {message.Text}");

                Program.ServiceManager?.StatisticsService?.IncreaseMessagesReveived();

                // Discard messages from bots
                if (message.From.IsBot)
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
                if(message.Type != MessageType.Text)
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

                    Program.ServiceManager?.StatisticsService?.IncreaseCommandsReceived();

                    _logger.Trace("Detected command");
                    await _commandManager.ProcessMessage(_client, message);
                }
                else
                {
                    // Reply ?
                    _logger.Trace("Detected message");

                    // Force reply possible
                    if (message.ReplyToMessage != null)
                    {
                        var replyToMessageId = message.ReplyToMessage.MessageId;
                        _logger.Trace($"Message {message.MessageId} is a reply to {replyToMessageId}. Checking for Force Reply ...");

                        lock(_cacheLock)
                        {
                            if(_cache.ContainsKey(message.Chat.Id))
                            {
                                var cache = GetCache(message.ReplyToMessage);
                                if (cache.MessageId == replyToMessageId)
                                {
                                    _logger.Trace($"Detected {replyToMessageId} as reply. Invoking action ...");
                                    // Reset ForceReply flag
                                    cache.ForceReply = false;
                                    // Invoke Event Handler
                                    cache.ForceReplyInstance.OnReplyReceived(_client, message);
                                }
                            }
                            else
                            {
                                _logger.Trace($"Not awaiting a reply to {replyToMessageId}");
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
                Program.ServiceManager?.StatisticsService?.IncreaseMessagesSent();
                await _client.SendTextMessageAsync(message.Chat.Id, Program.LanguageManager.GetTranslation("GeneralError"), replyToMessageId: message.MessageId);
            }
        }

        private async Task OnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            try
            {
                _logger.Trace($"Received CallbackQuery from {callbackQuery.From.Username}");

                var data = callbackQuery.Data;
                var message = callbackQuery.Message;

                lock (_cache)
                {
                    if (_cache.ContainsKey(message.Chat.Id))
                    {
                        var cache = _cache[message.Chat.Id];

                        if (HandlePagination(message, cache, data))
                        {
                            return;
                        }

                        var task = cache.QueryCallbackInstance.OnCallbackQueryReceived(_client, callbackQuery);
                        task.Wait();

                        if (!task.Result)
                        {
                            _logger.Warn($"Failed to execute callback for '{cache.QueryCallbackInstance}'");
                        }
                    }
                    else
                    {
                        _logger.Debug($"No callback found for message {message.MessageId}");
                        _client.DeleteMessageAsync(message.Chat, message.MessageId);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to process CallbackQuery");
                Program.ServiceManager.StatisticsService.IncreaseMessagesSent();
                await _client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, Program.LanguageManager.GetTranslation("GeneralError"));
            }
        }

        private bool HandlePagination(Message message, Cache cache, string data)
        {
            if (data == DisabledData)
            {
                _logger.Trace("Stupid user clicked on disabled button ... Ignoring the twat");
                return true;
            }

            if (data == NextData || data == PreviousData)
            {
                if (data == NextData)
                {
                    cache.Page++;
                }
                else
                {
                    cache.Page--;
                }

                var updateTask = UpdatePageination(message);
                updateTask.Wait();

                // Do not invoke any further tasks
                return true;
            }

            if(data == CancelData)
            {
                // Cancel Pagination
                _logger.Trace("Cancellation requested");

                _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                return true;
            }

            if(data.StartsWith(SelectData))
            {
                var selection = data.Substring(SelectData.Length, data.Length - SelectData.Length);
                if(!int.TryParse(selection, out var selectedIndex))
                {
                    // How ?
                    _logger.Warn($"Failed to parse '{selection}' to int");
                    _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }

                try
                {
                    var task = cache.PageSelectionFunction(_client, message, selectedIndex);
                    task.Wait();
                    
                    if(!task.Result)
                    {
                        _logger.Warn("Failed to process selection");
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to execute selection function");
                    _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                }

                return true;
            }

            return false;
        }
    }
}
