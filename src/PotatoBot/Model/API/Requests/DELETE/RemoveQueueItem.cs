using PotatoBot.Model.API.Requests;

namespace PotatoBot.Model.API.Requests.DELETE
{
    public class RemoveQueueItem : RequestBase
    {
        public uint Id { get; set; }
        public bool RemoveFromClient { get; set; }
        public bool Blocklist { get; set; }

        public RemoveQueueItem(uint id, bool removeFromClient, bool blocklist)
        {
            Id = id;
            RemoveFromClient = removeFromClient;
            Blocklist = blocklist;
        }

        public override string ToGet()
        {
            return $"removeFromClient={(RemoveFromClient ? "true" : "false")}&blocklist={(Blocklist ? "true" : "false")}";
        }
    }
}
