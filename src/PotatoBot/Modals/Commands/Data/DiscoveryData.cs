using System.Collections.Generic;

namespace PotatoBot.Modals.Commands.Data
{
	public class DiscoveryData : IData
	{
		public ServarrType SelectedSearch { get; set; }
		public IServarr API { get; set; }

		public IEnumerable<IServarrItem> SearchResults { get; set; }
	}
}
