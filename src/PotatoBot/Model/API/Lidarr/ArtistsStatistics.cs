namespace PotatoBot.Model.API.Lidarr
{
    public class ArtistsStatistics : MediaStatistics
    {
        public uint AlbumCount { get; set; }
        public float PercentOfTracks { get; set; }
        public ulong TotalTrackCount { get; set; }
        public ulong TrackCount { get; set; }
        public ulong TrackFileCount { get; set; }
    }
}
