namespace PotatoBot.Modals.API.Lidarr
{
    public class LidarrQueueItem : QueueItem
    {
        public ulong ArtistId { get; set; }
        public ulong AlbumId { get; set; }

        public override string GetQueueTitle()
        {
            var albumInfo = Program.ServiceManager.Lidarr.GetAlbumInfo(ArtistId, AlbumId);
            return $"{albumInfo.Artist.ArtistName} : {albumInfo.Title}";
        }
    }
}
