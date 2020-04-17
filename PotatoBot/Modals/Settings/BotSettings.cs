﻿namespace PotatoBot.Modals.Settings
{
    public class BotSettings
    {
        public TelegramSettings Telegram { get; set; } = new TelegramSettings();

        public WebhookSettings Webhook { get; set; } = new WebhookSettings();

        public EntertainmentSettings Radarr { get; set; } = new EntertainmentSettings();
        public EntertainmentSettings Sonarr { get; set; } = new EntertainmentSettings();
        public EntertainmentSettings Lidarr { get; set; } = new EntertainmentSettings();

        public PlexSettings Plex { get; set; } = new PlexSettings();
    }
}
