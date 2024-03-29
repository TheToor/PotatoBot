﻿namespace PotatoBot.Model
{
    public class Statistics
    {
        public long MessagesSent { get; set; }
        public long MessagesReveived { get; set; }
        public long MessagesProcessed { get; set; }
        public long CommandsReceived { get; set; }
        public long CommandsProcessed { get; set; }
        public long Searches { get; set; }
        public long Adds { get; set; }

        public ulong WebhooksReceived { get; set; }
        public ulong WebhooksProcessed { get; set; }
    }
}
