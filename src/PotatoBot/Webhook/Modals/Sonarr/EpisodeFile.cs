namespace PotatoBot.Webhook.Modals.Sonarr
{
	public class EpisodeFile
	{
		public int Id { get; set; }
		public string RelativePath { get; set; }
		public string Path { get; set; }
		public string Quality { get; set; }
		public int QualityVersion { get; set; }
	}
}
