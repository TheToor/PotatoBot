using PotatoBot.Model.Settings;
using System.Collections.Generic;

namespace PotatoBot.Model.Webhook.Telegram
{
    public class InitModel
    {
        public IEnumerable<EntertainmentSettings>? Sonarr { get; set; }
        public IEnumerable<EntertainmentSettings>? Radarr { get; set; }
        public IEnumerable<EntertainmentSettings>? Lidarr { get; set; }
    }
}
