using PotatoBot.Modals.API.Sonarr;

namespace PotatoBot.Modals.API.Requests.POST
{
    public class AddSeries : Series
    {
        // Additinal properties required for add
        public string RootFolderPath { get; set; }
        public SeriesAddOption AddOptions { get; set; }

        public AddSeries(Series series)
        {
            // Minimal required things for Sonarr

            TvDbId = series.TvDbId;
            Title = series.Title;
            TitleSlug = series.TitleSlug;
            Images = series.Images;
            Seasons = series.Seasons;

            QualityProfileId = Program.Settings.Sonarr.QualityProfile;
            LanguageProfileId = Program.Settings.Sonarr.LanguageProfile;
            RootFolderPath = Program.Settings.Sonarr.DownloadPath;

            SeasonFolder = true;
            Monitored = true;
            AddOptions = new SeriesAddOption()
            {
                IgnoreEpisodesWithFiles = true,
                IgnoreEpisodesWithoutFiles = false,
                SearchForMissingEpisodes = true
            };
        }
    }
}
