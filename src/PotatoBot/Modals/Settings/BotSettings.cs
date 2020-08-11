using System.Collections.Generic;

namespace PotatoBot.Modals.Settings
{
    public class BotSettings
    {
        public TelegramSettings Telegram { get; set; } = new TelegramSettings();

        public WebhookSettings Webhook { get; set; } = new WebhookSettings();

        public EntertainmentSettings Radarr { get; set; } = new EntertainmentSettings();
        public EntertainmentSettings Sonarr { get; set; } = new EntertainmentSettings();
        public EntertainmentSettings Lidarr { get; set; } = new EntertainmentSettings();

        public List<PlexSettings> Plex { get; set; } = new List<PlexSettings>();

        public List<SABnzbdSettings> SABnzbd { get; set; } = new List<SABnzbdSettings>();

        public List<string> CORSUrls { get; set; } = new List<string>();
    }
}
