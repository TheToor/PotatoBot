using PotatoBot.Model.Webhook;

namespace PotatoBot.Model.Webhook.Radarr
{
    public class Rename : RequestBase
    {
        public Movie Movie { get; set; }
    }
}
