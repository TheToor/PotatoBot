using PotatoBot.Modals.Webhook;

namespace PotatoBot.Webhook.Modals.Lidarr
{
    public class LidarrRequestBase : RequestBase
    {
        public Artist Artist { get; set; }
    }
}
