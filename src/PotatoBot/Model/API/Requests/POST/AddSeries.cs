using PotatoBot.Model.API.Sonarr;
using PotatoBot.Services;

namespace PotatoBot.Model.API.Requests.POST
{
    public class AddSeries : Series
    {
        // Additinal properties required for add
        public string RootFolderPath { get; set; }
        public SeriesAddOption AddOptions { get; set; }

        public AddSeries(SonarrService service, Series series)
        {
            // Minimal required things for Sonarr

            TvDbId = series.TvDbId;
            Title = series.Title;
            TitleSlug = series.TitleSlug;
            Images = series.Images;
            Seasons = series.Seasons;

            QualityProfileId = service.Settings.QualityProfile;
            LanguageProfileId = service.Settings.LanguageProfile;
            RootFolderPath = service.Settings.DownloadPath;

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
