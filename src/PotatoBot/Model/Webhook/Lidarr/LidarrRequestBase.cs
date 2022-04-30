using PotatoBot.Model.Webhook;

namespace PotatoBot.Model.Webhook.Lidarr
{
    public class LidarrRequestBase : RequestBase
    {
        public Artist Artist { get; set; }
    }
}
