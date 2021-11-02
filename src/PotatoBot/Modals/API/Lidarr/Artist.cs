using System.Collections.Generic;
using System.Linq;

namespace PotatoBot.Modals.API.Lidarr
{
	public class Artist : IServarrItem
	{
		public string Title
		{
			get
			{
				return ArtistName;
			}

			set
			{
				ArtistName = value;
			}
		}
		public string PageTitle => $"<b>{ArtistName}</b>\n{Overview}\n\n";
		public ushort Year { get; set; }

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
