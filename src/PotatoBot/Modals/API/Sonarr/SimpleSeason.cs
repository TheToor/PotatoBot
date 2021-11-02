namespace PotatoBot.Modals.API.Sonarr
{
	public class SimpleSeason
	{
		public bool Monitored { get; set; }
		public int SeasonNumber { get; set; }
		public SeriesStatistics Statistics { get; set; }
	}
}
