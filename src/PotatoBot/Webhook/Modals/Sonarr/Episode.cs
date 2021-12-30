using System;
namespace PotatoBot.Webhook.Modals.Sonarr
{
    public class Episode
	{
		public int Id { get; set; }
		public int EpisodeNumber { get; set; }
		public int SeasonNumber { get; set; }
		public string Title { get; set; }
		public DateTime AirDate { get; set; }
		public DateTime AirDateUtc { get; set; }
		public string Quality { get; set; }
		public int QualityVersion { get; set; }
	}
}
