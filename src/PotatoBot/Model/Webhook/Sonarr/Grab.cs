using PotatoBot.Model.Webhook;
using System.Collections.Generic;

namespace PotatoBot.Model.Webhook.Sonarr
{
    public class Grab : RequestBase
    {
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public Release Release { get; set; }
    }
}
