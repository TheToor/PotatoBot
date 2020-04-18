namespace PotatoBot.Modals.API.Sonarr
{
    public class SimpleSeason
    {
        public bool Monitored { get; set; }
        public int SeasonNumber { get; set; }
        public Statistics Statistics { get; set; }
    }
}
