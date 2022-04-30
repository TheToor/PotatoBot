using System.Collections.Generic;

namespace PotatoBot.Model
{
    internal interface IServarrSupportsDiscovery
    {
        IEnumerable<IServarrItem> GetDiscoveryQueue();
    }
}
