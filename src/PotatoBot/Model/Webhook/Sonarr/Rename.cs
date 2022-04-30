using PotatoBot.Model.Webhook;

namespace PotatoBot.Model.Webhook.Sonarr
{
    public class Rename : RequestBase
    {
        public Series Series { get; set; }
    }
}
