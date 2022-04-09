using PotatoBot.Controllers;
using PotatoBot.Services;

namespace PotatoBot.Modals.API.Lidarr
{
    public class LidarrQueueItem : QueueItem
    {
        public ulong ArtistId { get; set; }
        public ulong AlbumId { get; set; }

        public LidarrQueueItem(APIBase api) : base(api) { }

        public override string GetQueueTitle()
        {
            var albumInfo = (API as LidarrService).GetAlbumInfo(ArtistId, AlbumId);
            return $"{albumInfo.Artist.ArtistName} : {albumInfo.Title}";
        }
    }
}
