using System.Collections.Generic;

namespace PotatoBot.Modals.API.Lidarr
{
    public class Artist
    {
        public ulong Id { get; set; }
        public string ForeignArtistId { get; set; }

        public uint QualityProfileId { get; set; }
        public uint LanguageProfileId { get; set; }
        public uint MetadataProfileId { get; set; }

        public bool AlbumFolder { get; set; }
        public bool Monitored { get; set; }

        public string Status { get; set; }

        public string ArtistName { get; set; }
        public string Overview { get; set; }

        public string RemotePoster { get; set; }
        public int TadbId { get; set; }
        public int DiscogsId { get; set; }

        public string Path { get; set; }
        public string RootFolderPath { get; set; }

        public ArtistsStatistics Statistics { get; set; }

        public List<Image> Images { get; set; } = new List<Image>();
    }
}
