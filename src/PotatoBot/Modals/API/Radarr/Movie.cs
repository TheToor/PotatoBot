using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PotatoBot.Modals.API.Radarr
{
    public class Movie : IEqualityComparer<Movie>
    {
        public int Id { get; set; }
        // Radarr does not (yet?) have languge profiles. Language is specified inside the quality profile
        //public uint LanguageProfileId { get; set; }
        public uint QualityProfileId { get; set; }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public long SizeOnDisk { get; set; }
        public string Status { get; set; }
        public string Overview { get; set; }
        public DateTime InCinemas { get; set; }
        public DateTime PhysicalRelease { get; set; }
        public List<Image> Images { get; set; } = new List<Image>();
        public string Website { get; set; }
        public bool Downloaded { get; set; }
        public ushort Year { get; set; }
        public bool HasFile { get; set; }
        public string YouTubeTrailerId { get; set; }
        public string Studio { get; set; }
        public string Path { get; set; }
        public bool Monitored { get; set; }
        public ushort Runtime { get; set; }
        public DateTime LastInfoSync { get; set; }
        public string CleanTitle { get; set; }
        public string IMDBId { get; set; }
        public int TMDBId { get; set; }
        public string TitleSlug { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Added { get; set; }
        public Rating Ratings { get; set; }
        public List<AlternativeTitle> AlternativeTitles { get; set; }

        public bool Equals([AllowNull] Movie x, [AllowNull] Movie y)
        {
            if (x == null) return false;
            if (y == null) return false;
            if (ReferenceEquals(x, y)) return true;

            if (x.TMDBId == y.TMDBId)
                return true;
            if (x.IMDBId == y.IMDBId)
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Movie obj)
        {
            return TMDBId.GetHashCode() ^ IMDBId.GetHashCode();
        }
    }
}
