﻿using PotatoBot.Extensions;
using PotatoBot.Managers;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Services
{
    internal class TelegramService : IService
    {
        const string PreviousData = "Previous";
        const string NextData = "Next";
        const string DisabledData = "Disabled";

        public string Name => "Telegram";

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private TelegramSettings _settings => Program.Settings.Telegram;

        private TelegramBotClient _client;

        private CommandManager _commandManager;

        // Thread lock for cache
        private object _cacheLock = new object();
        // Cache to store data "inside" of a telegram chat
        private Dictionary<long, Cache> _cache = new Dictionary<long, Cache>();
        // Timer to invalidate cache
        private System.Timers.Timer _cacheTimer;
        // Time until a message "expires" in hours
        private uint _cacheInvalidationTime = 24;

        internal TelegramService()
        {
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

                _logger.Trace("Setting events ...");
                _client.OnMessage += OnNewMessage;
                _client.OnReceiveError += OnReceiveError;
                _client.OnReceiveGeneralError += OnReceiveGeneralError;
                _client.OnCallbackQuery += OnCallbackQueryReceived;

                _logger.Trace("Initializing command manager ...");
                _commandManager = new CommandManager();

                _logger.Trace("Initializing cache ...");
                _cacheTimer = new System.Timers.Timer(1000 * 60 * 60);
                _cacheTimer.AutoReset = true;
                _cacheTimer.Elapsed += ValidateCache;
                _cacheTimer.Start();

                _logger.Trace("Starting ...");
                _client.StartReceiving();

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
            _client.StopReceiving();
            return true;
        }

        internal async Task<Message> SimpleReplyToMessage(Message message, string text)
        {
            _logger.Trace($"Sending '{text}' to {message.From.Username}");
            return await _client.SendTextMessageAsync(message.Chat, text, replyToMessageId: message.MessageId);
        }

        internal async Task<Message> ForceReply(IReplyCallback caller, Message message, string title, bool update = true)
        {
            var cache = GetCache(message);
            cache.ForceReply = true;
            cache.ForceReplyInstance = caller;
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
            var sentMessage = await ReplyWithMarkup(caller, message, title, markup);
            CacheOrUpdate(sentMessage, data);
            return sentMessage;
        }
        internal async Task<Message> ReplyWithMarkup(IQueryCallback caller, Message message, string title, IReplyMarkup markup)
        {
            var sentMessage = await _client.SendTextMessageAsync(message.Chat, title, replyMarkup: markup, replyToMessageId: message.MessageId);
            var cache = GetCache(sentMessage);
            cache.QueryCallbackInstance = caller;
            return sentMessage;
        }

        internal async Task ReplyWithPageination<T>(Message message, string title, List<T> list, Func<object, string> formatFunction)
        {
            _logger.Trace($"Starting pageination for message {message.MessageId} in chat {message.Chat.Id}");

            var cache = GetCache(message);

            _logger.Trace("Preparing page ...");
            cache.PageTitle = title;
            cache.Page = 0;
            cache.PageItemList = list.Cast<object>().ToList();
            cache.PageFormatFunction = formatFunction;

            await UpdatePageination(message.ReplyToMessage, true);
        }

        internal async Task UpdatePageination(Message message, bool create = false)
        {
            _logger.Trace($"Updating pageination of message {message.MessageId}");

            var cache = GetCache(message);
            var text = string.Empty;

            var page = cache.PageItemList.TakePaged(cache.Page, cache.PageSize);
            foreach (var item in page.Items)
            {
                text += cache.PageFormatFunction(item);
            }

            _logger.Trace("Building button layout ...");
            var buttons = new List<InlineKeyboardButton>();
            if (page.PreviousPossible)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("<<", PreviousData));
            }
            else if(page.NextPossible)
            {
                // If next is possible add a spacing button
                buttons.Add(InlineKeyboardButton.WithCallbackData("  ", DisabledData));
            }

            if (page.NextPossible)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData(">>", NextData));
            }
            else if(page.PreviousPossible)
            {
                // if previous is possbile add a spacing button
                buttons.Add(InlineKeyboardButton.WithCallbackData("  ", DisabledData));
            }

            var keyboardMarkup = new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                buttons
            });

            var messageText = $"{cache.PageTitle}\n\n{text}";
            _logger.Trace($"Sending pageination ({messageText.Length})");

            if (create)
            {
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

                return default(T);
            }
        }

        internal List<List<InlineKeyboardButton>> GetDefaultEntertainmentInlineKeyboardButtons()
        {
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            if (Program.Settings.Radarr.Enabled)
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Movies", $"{(int)SearchType.Movie}")
                });
            }

            if (Program.Settings.Sonarr.Enabled)
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Series", $"{(int)SearchType.Series}")
                });
            }

            if (Program.Settings.Lidarr.Enabled)
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Artists", $"{(int)SearchType.Artist}")
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

        private bool IsValidUser(int userId)
        {
            if(_settings.Users.Contains(userId) || _settings.Admins.Contains(userId))
            {
                return true;
            }
            return false;
        }

        private async void OnNewMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var user = e.Message.From;
                var message = e.Message;

                _logger.Trace($"Received new message from [{user.Id}] {user.Username}: {message.Text}");

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
            }
        }

        private async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            _logger.Trace($"Received CallbackQuery from {e.CallbackQuery.From.Username}");

            var data = e.CallbackQuery.Data;
            var message = e.CallbackQuery.Message;

            lock(_cache)
            {
                if(_cache.ContainsKey(message.Chat.Id))
                {
                    var cache = _cache[message.Chat.Id];

                    if(data == DisabledData)
                    {
                        _logger.Trace("Stupid user clicked on disabled button ... Ignoring the twat");
                        return;
                    }

                    if(data == NextData || data == PreviousData)
                    {
                        if(data == NextData)
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
                        return;
                    }

                    var task = cache.QueryCallbackInstance.OnCallbackQueryReceived(_client, e);
                    task.Wait();

                    if(!task.Result)
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

        private void OnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            _logger.Warn($"Error in TelegramManager: {e.ApiRequestException}");
        }

        private void OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            _logger.Warn($"Error in TelegramManager: {e.Exception}");
        }
    }
}
