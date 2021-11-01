using System.Collections.Generic;

namespace PotatoBot.Modals.Settings
{
    public class BotSettings
    {
        public TelegramSettings Telegram { get; set; } = new TelegramSettings();

        public WebhookSettings Webhook { get; set; } = new WebhookSettings();

        public List<EntertainmentSettings> Radarr { get; set; } = new List<EntertainmentSettings>();
        public List<EntertainmentSettings> Sonarr { get; set; } = new List<EntertainmentSettings>();
        public List<EntertainmentSettings> Lidarr { get; set; } = new List<EntertainmentSettings>();

        public List<PlexSettings> Plex { get; set; } = new List<PlexSettings>();

        public List<SABnzbdSettings> SABnzbd { get; set; } = new List<SABnzbdSettings>();

        public List<string> CORSUrls { get; set; } = new List<string>();

        public bool DebugNoPreview { get; set; }
        public bool AddPicturesToSearch { get; set; }
    }
}
