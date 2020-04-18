namespace PotatoBot.Modals.API.Sonarr
{
    public class SonarrQueueItem : QueueItem
    {
        public ulong EpisodeId { get; set; }
        public ulong SeriesId { get; set; }

        public override string GetQueueTitle()
        {
            var episodeInfo = Program.ServiceManager.Sonarr.GetEpisodeInfo(SeriesId, EpisodeId);
            return $"{episodeInfo.Series.Title} : {episodeInfo.Title}";
        }
    }
}
