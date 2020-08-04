using System.Linq;

namespace PotatoBot.Modals.API.Sonarr
{
    public class BasicSeries
    {
        public string Title { get; set; }
        public SeriesStatistics Statistics { get; set; }
        public Image Poster { get; set; }

        public BasicSeries(Series series)
        {
            Title = series.Title;
            Statistics = series.Statistics;
            Poster = series.Images.FirstOrDefault((i) => i.CoverType == "poster");
        }
    }
}
