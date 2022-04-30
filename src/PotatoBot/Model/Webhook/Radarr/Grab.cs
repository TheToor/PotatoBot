using PotatoBot.Model.Webhook;

namespace PotatoBot.Model.Webhook.Radarr
{
    public class Grab : RequestBase
    {
        public Movie Movie { get; set; }
        public RemoteMovie RemoteMovie { get; set; }
        public Release Release { get; set; }
    }
}
