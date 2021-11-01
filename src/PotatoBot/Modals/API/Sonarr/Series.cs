using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PotatoBot.Modals.API.Sonarr
{
    public class Series : IServarrItem, IEqualityComparer<Series>
    {
        public ulong Id { get; set; }
        public uint LanguageProfileId { get; set; }
        public uint QualityProfileId { get; set; }

        public string Title { get; set; }
        public string PageTitle => $"<b>{Year} - {Title}</b>\n{Overview}\n\n";

        public DateTime Added { get; set; }
        public string AirTime { get; set; }
        public string Certification { get; set; }
        public string CleanTitle { get; set; }
        public bool Ended { get; set; }
        public DateTime FirstAired { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<Image> Images { get; set; } = new List<Image>();
        public string ImdbId { get; set; }
        public bool Monitored { get; set; }
        public string Network { get; set; }
        public DateTime NextAiring { get; set; }
        public string Overview { get; set; }
        public string Path { get; set; }
        public DateTime PreviousAiring { get; set; }
        public Rating Ratings { get; set; }
        public int Runtime { get; set; }
        public bool SeasonFolder { get; set; }
        public List<SimpleSeason> Seasons { get; set; }
        // This should be an enum
        public string SeriesType { get; set; }
        public string SortTitle { get; set; }
        public SeriesStatistics Statistics { get; set; }
        // This should be an enum
        public string Status { get; set; }
        // To check
        public List<string> Tags { get; set; }
        public string TitleSlug { get; set; }
        public int TvMazeId { get; set; }
        public int TvRageId { get; set; }
        public int TvDbId { get; set; }
        public bool UseSceneNumbering { get; set; }
        public ushort Year { get; set; }

        public bool Equals([AllowNull] Series x, [AllowNull] Series y)
        {
            if (x == null) return false;
            if (y == null) return false;
            if (ReferenceEquals(x, y)) return true;

            if (x.TvDbId == y.TvDbId)
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Series obj)
        {
            return TvDbId.GetHashCode();
        }

        public string GetPosterUrl()
        {
            if(Images.Count > 0)
            {
                if(Images.Any(i => i.CoverType == MediaCoverTypes.Poster))
                {
                    return Images.FirstOrDefault(i => i.CoverType == MediaCoverTypes.Poster)?.RemoteUrl;
                }
                return Images.First().RemoteUrl;
            }
            return string.Empty;
        }
    }
}
