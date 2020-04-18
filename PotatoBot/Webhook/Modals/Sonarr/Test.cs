using System.Collections.Generic;

namespace PotatoBot.Webhook.Modals.Sonarr
{
    public class Test : RequestBase
    {
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
    }
}
