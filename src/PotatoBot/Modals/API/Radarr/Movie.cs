using System;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.Radarr
{
    public class Movie
    {
        public int Id { get; set; }
        // Radarr does not (yet?) have languge profiles. Language is specified inside the quality profile
        //public uint LanguageProfileId { get; set; }
        public uint QualityProfileId { get; set; }
        // Same as 'QualityProfileId' ??
        public ulong ProfileId { get; set; }

        public string Title { get; set; }
        public string SortTitle { get; set; }
        public long SizeOnDisk { get; set; }
        public string Status { get; set; }
        public string Overview { get; set; }
        public DateTime InCinemas { get; set; }
        public DateTime PhysicalRelease { get; set; }
        public List<Image> Images { get; set; }
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
    }
}
