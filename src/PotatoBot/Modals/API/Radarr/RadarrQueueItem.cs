namespace PotatoBot.Modals.API.Radarr
{
    public class RadarrQueueItem : QueueItem
    {
        public Movie Movie { get; set; }

        public override string GetQueueTitle()
        {
            return Movie?.Title ?? Title ?? "UNKNOWN";
        }
    }
}
