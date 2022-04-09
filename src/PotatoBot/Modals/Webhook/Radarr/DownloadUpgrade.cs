using PotatoBot.Modals.Webhook;

namespace PotatoBot.Webhook.Modals.Radarr
{
    public class DownloadUpgrade : RequestBase
    {
        public Movie Movie { get; set; }
        public RemoteMovie RemoteMovie { get; set; }
        public MovieFile MovieFile { get; set; }
        public bool IsUpgrade { get; set; }
    }
}
