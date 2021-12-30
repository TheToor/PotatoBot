namespace PotatoBot.Modals.API
{
    public class Image
	{
		// This should be an enum
		public MediaCoverTypes CoverType { get; set; }
		public string RemoteUrl { get; set; }
		public string Url { get; set; }
	}
}
