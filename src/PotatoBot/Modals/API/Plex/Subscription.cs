using System.Collections.Generic;

namespace PotatoBot.Modals.API.Plex
{
    public class Subscription
    {
        public bool Active { get; set; }
        public string Status { get; set; }
        public string Plan { get; set; }
        public List<string> Features { get; set; }
    }
}
