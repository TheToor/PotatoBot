namespace PotatoBot.Modals.API.Sonarr
{
    public class SeriesAddOption
    {
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
        public bool SearchForMissingEpisodes { get; set; }
    }
}
