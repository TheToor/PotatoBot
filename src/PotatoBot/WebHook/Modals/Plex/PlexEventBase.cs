namespace PotatoBot.WebHook.Modals.Plex
{
    public class PlexEventBase
    {
        private string _event;
        public string Event
        {
            get => _event;
            set
            {
                _event = value;

                switch (_event)
                {
                    case "library.on.deck":
                        EventType = EventType.NewOnDeck;
                        break;
                    case "library.new":
                        EventType = EventType.NewInLibrary;
                        break;

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

                    case "admin.database.backup":
                        EventType = EventType.DatabaseBackup;
                        break;
                    case "admin.database.corrupted":
                        EventType = EventType.DatabaseCorrupted;
                        break;
                    case "device.new":
                        EventType = EventType.NewDevice;
                        break;
                    case "playback.started":
                        EventType = EventType.PlaybackStarted;
                        break;
                }
            }
        }
        internal EventType EventType { get; set; }
        public bool User { get; set; }
        public bool Owner { get; set; }
    }
}
