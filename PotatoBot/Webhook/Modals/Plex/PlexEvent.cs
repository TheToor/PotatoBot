namespace PotatoBot.Webhook.Modals.Plex
{
    public class PlexEvent
    {
        private string _event;
        public string Event
        {
            get => _event;
            set
            {
                _event = value;

                switch(_event)
                {
                    case "media.play":
                        EventType = EventType.Play;
                        break;
                    case "media.pause":
                        EventType = EventType.Pause;
                        break;
                    case "media.resume":
                        EventType = EventType.Resume;
                        break;
                    case "media.stop":
                        EventType = EventType.Stop;
                        break;
                    case "media.scrobble":
                        EventType = EventType.Scrobble;
                        break;
                    case "media.rate":
                        EventType = EventType.Rate;
                        break;
                }
            }
        }
        internal EventType EventType { get; set; }
        public bool User { get; set; }
        public bool Owner { get; set; }
        public Account Account { get; set; }
        public Server Server { get; set; }
        public Player Player { get; set; }
        public Metadata Metadata { get; set; }
    }
}
