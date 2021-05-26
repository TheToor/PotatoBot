using PotatoBot.API;
using PotatoBot.Services;

namespace PotatoBot.Modals.API.Sonarr
{
    public class SonarrQueueItem : QueueItem
    {
        public ulong EpisodeId { get; set; }
        public ulong SeriesId { get; set; }

        public SonarrQueueItem(APIBase api) : base(api) { }

        public override string GetQueueTitle()
        {
            var episodeInfo = (API as SonarrService).GetEpisodeInfo(SeriesId, EpisodeId);
            return $"{episodeInfo.Series.Title} : {episodeInfo.Title}";
        }
    }
}
