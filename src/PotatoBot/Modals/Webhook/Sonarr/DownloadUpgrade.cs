using PotatoBot.Modals.Webhook;
using System.Collections.Generic;

namespace PotatoBot.Webhook.Modals.Sonarr
{
    public class DownloadUpgrade : RequestBase
    {
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
