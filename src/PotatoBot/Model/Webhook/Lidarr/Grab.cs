using System.Collections.Generic;

namespace PotatoBot.Model.Webhook.Lidarr
{
    public class Grab : LidarrRequestBase
    {
        public List<Album> Albums { get; set; }
        public Release Release { get; set; }
    }
}
