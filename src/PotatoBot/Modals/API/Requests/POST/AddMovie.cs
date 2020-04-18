using PotatoBot.Modals.API.Radarr;

namespace PotatoBot.Modals.API.Requests.POST
{
    public class AddMovie : Movie
    {
        public string RootFolderPath { get; set; }
        public MovieAddOptions AddOptions { get; set; }

        public AddMovie(Movie movie)
        {
            Title = movie.Title;
            TitleSlug = movie.TitleSlug;
            Images = movie.Images;
            TMDBId = movie.TMDBId;
            Year = movie.Year;

            QualityProfileId = Program.Settings.Radarr.QualityProfile;
            ProfileId = Program.Settings.Radarr.QualityProfile;
            RootFolderPath = Program.Settings.Radarr.DownloadPath;

            Monitored = true;
            AddOptions = new MovieAddOptions()
            {
                SearchForMovie = true
            };
        }
    }
}
