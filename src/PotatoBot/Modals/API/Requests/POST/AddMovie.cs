using PotatoBot.Modals.API.Radarr;
using PotatoBot.Services;

namespace PotatoBot.Modals.API.Requests.POST
{
    public class AddMovie : Movie
    {
        public string RootFolderPath { get; set; }
        public string MinimumAvailability { get; set; }
        public MovieAddOptions AddOptions { get; set; }

        public AddMovie(RadarrService service, Movie movie)
        {
            Title = movie.Title;
            TitleSlug = movie.TitleSlug;
            Images = movie.Images;
            IMDBId = movie.IMDBId;
            TMDBId = movie.TMDBId;
            Year = movie.Year;

            QualityProfileId = service.Settings.QualityProfile;
            RootFolderPath = service.Settings.DownloadPath;

            Monitored = true;
            AddOptions = new MovieAddOptions()
            {
                SearchForMovie = true
            };

            MinimumAvailability = "Announced";
        }
    }
}
