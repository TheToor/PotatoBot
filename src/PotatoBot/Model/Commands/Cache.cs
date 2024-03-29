﻿using PotatoBot.Model.Commands.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Model.Commands
{
    public class Cache
    {
        public DateTime Created { get; } = DateTime.Now;
        public DateTime LastAccessed { get; set; } = DateTime.Now;

        public bool ForceReply { get; set; }
        public IReplyCallback? ForceReplyInstance { get; set; }

        public IQueryCallback? QueryCallbackInstance { get; set; }

        public IData? Data { get; set; }

        public ChatId? ChatId { get; set; }
        public int MessageId { get; set; }

        public string? PageTitle { get; set; }
        public int PageSize { get; set; } = 3;
        public int Page { get; set; }
        public IEnumerable<IServarrItem>? PageItemList { get; set; }
        public Func<TelegramBotClient, Message, int, Task<bool>>? PageSelectionFunction { get; set; }
    }
}
