using System.Collections.Generic;

namespace PotatoBot.Modals
{
    internal interface IServarrSupportsDiscovery
	{
		IEnumerable<IServarrItem> GetDiscoveryQueue();
	}
}
