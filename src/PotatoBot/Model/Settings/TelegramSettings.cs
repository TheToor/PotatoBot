﻿using System.Collections.Generic;

namespace PotatoBot.Model.Settings
{
    public class TelegramSettings
    {
        public string BotToken { get; set; }
        public string AlertBotToken { get; set; }

        public List<long> Admins { get; set; }
        public List<long> Users { get; set; }

        public string DateTimeFormat { get; set; }
    }
}
