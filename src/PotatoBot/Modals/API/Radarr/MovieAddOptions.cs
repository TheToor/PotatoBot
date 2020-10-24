namespace PotatoBot.Modals.API.Radarr
{
    public class MovieAddOptions
    {
        public bool SearchForMovie { get; set; }
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
    }
}
