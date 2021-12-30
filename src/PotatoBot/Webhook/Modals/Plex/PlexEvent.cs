namespace PotatoBot.WebHook.Modals.Plex
{
    public class PlexEvent : PlexEventBase
	{
		public Account Account { get; set; }
		public Server Server { get; set; }
		public Player Player { get; set; }
		public Metadata Metadata { get; set; }
	}
}
