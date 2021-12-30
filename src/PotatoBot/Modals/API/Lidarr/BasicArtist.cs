using System;
using System.Linq;

namespace PotatoBot.Modals.API.Lidarr
{
    public class BasicArtist
    {
        public string ArtistName { get; set; }
        public ArtistsStatistics Statistics { get; set; }
        public Image Poster { get; set; }

        public BasicArtist(IServarrItem item)
        {
            var artist = item as Artist ?? throw new ArgumentNullException(nameof(item));

            ArtistName = artist.ArtistName;
            Statistics = artist.Statistics;
            Poster = artist.Images?.FirstOrDefault((i) => i.CoverType == MediaCoverTypes.Poster);
        }
    }
}
