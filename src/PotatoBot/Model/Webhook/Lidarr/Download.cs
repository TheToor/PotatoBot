using System.Collections.Generic;

namespace PotatoBot.Model.Webhook.Lidarr
{
    public class Download : LidarrRequestBase
    {
        public List<Track> Tracks { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
