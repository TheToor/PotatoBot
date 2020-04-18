using PotatoBot.Modals.API.Lidarr;

namespace PotatoBot.Modals.API.Requests.POST
{
    public class AddArtist : Artist
    {
        public ArtistAddOptions AddOptions { get; set; }

        public AddArtist(Artist artist)
        {
            Id = artist.Id;
            ForeignArtistId = artist.ForeignArtistId;

            ArtistName = artist.ArtistName;

            Status = artist.Status;
            RemotePoster = artist.RemotePoster;
            TadbId = artist.TadbId;
            DiscogsId = artist.DiscogsId;

            Overview = artist.Overview;

            QualityProfileId = Program.Settings.Lidarr.QualityProfile;
            RootFolderPath = Program.Settings.Lidarr.DownloadPath;

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
