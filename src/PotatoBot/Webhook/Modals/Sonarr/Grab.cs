using System.Collections.Generic;

namespace PotatoBot.Webhook.Modals.Sonarr
{
	public class Grab : RequestBase
	{
		public Series Series { get; set; }
		public List<Episode> Episodes { get; set; }
		public Release Release { get; set; }
	}
}
