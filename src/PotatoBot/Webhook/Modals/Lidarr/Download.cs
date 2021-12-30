using System.Collections.Generic;

namespace PotatoBot.Webhook.Modals.Lidarr
{
    public class Download : LidarrRequestBase
	{
		public List<Track> Tracks { get; set; }
		public bool IsUpgrade { get; set; }
	}
}
