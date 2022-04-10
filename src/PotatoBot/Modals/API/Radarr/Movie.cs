using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PotatoBot.Modals.API.Radarr
{
    public class Movie : IServarrItem, IDiscoveryItem, IEqualityComparer<Movie>
    {
        public ulong Id { get; set; }
        public uint QualityProfileId { get; set; }

        public string Title { get; set; }
        public string PageTitle => $"<b>{Year} - {Title}</b>\n{Overview}\n\n";

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

        public bool IsExcluded { get; set; }
        public bool IsExisting { get; set; }
        public bool IsRecommendation { get; set; }

        public bool Equals([AllowNull] Movie x, [AllowNull] Movie y)
        {
            if(x == null)
            {
                return false;
            }

            if(y == null)
            {
                return false;
            }

            if(ReferenceEquals(x, y))
            {
                return true;
            }

            if(x.TMDBId == y.TMDBId)
            {
                return true;
            }

            if(x.IMDBId == y.IMDBId)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode([DisallowNull] Movie obj)
        {
            return TMDBId.GetHashCode() ^ IMDBId.GetHashCode();
        }

        public string GetPosterUrl()
        {
            Image? selectedImage = default;

            if(Images.Count > 0)
            {
                if(Images.Any(i => i.CoverType == MediaCoverTypes.Poster))
                {
                    selectedImage = Images.FirstOrDefault(i => i.CoverType == MediaCoverTypes.Poster);
                }
                selectedImage = Images.First();
            }

            if(selectedImage != null)
            {
                if(!string.IsNullOrEmpty(selectedImage.RemoteUrl))
                {
                    return selectedImage.RemoteUrl;
                }
                else if(selectedImage.Url.StartsWith("http"))
                {
                    // In case of discovery the remote url is stored in the url ??????????????????????????????
                    return selectedImage.Url;
                }
            }

            return null;
        }
    }
}
