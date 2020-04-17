using System.Collections.Generic;

namespace PotatoBot.Modals.Settings
{
    public class TelegramSettings
    {
        public string BotToken { get; set; }

        public List<int> Admins { get; set; }
        public List<int> Users { get; set; }

        public string DateTimeFormat { get; set; }
    }
}
