using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Services;

namespace PotatoBot.Modals.API.Requests.POST
{
    public class AddArtist : Artist
	{
		public ArtistAddOptions AddOptions { get; set; }

		public AddArtist(LidarrService service, Artist artist)
		{
			Id = artist.Id;
			ForeignArtistId = artist.ForeignArtistId;

			ArtistName = artist.ArtistName;

			Status = artist.Status;
			RemotePoster = artist.RemotePoster;
			TadbId = artist.TadbId;
			DiscogsId = artist.DiscogsId;

			Overview = artist.Overview;

			QualityProfileId = service.Settings.QualityProfile;
			RootFolderPath = service.Settings.DownloadPath;

			LanguageProfileId = 1;
			MetadataProfileId = 1;

			Monitored = true;
			AddOptions = new ArtistAddOptions()
			{
				SearchForMissingAlbums = true
			};
		}
	}
}
