using System;
using System.Linq;

namespace PotatoBot.Modals.API.Sonarr
{
	public class BasicSeries
	{
		public string Title { get; set; }
		public SeriesStatistics Statistics { get; set; }
		public Image Poster { get; set; }

		public BasicSeries(IServarrItem item)
		{
			var series = item as Series ?? throw new ArgumentNullException(nameof(item));

			Title = series.Title;
			Statistics = series.Statistics;
			Poster = series.Images.FirstOrDefault((i) => i.CoverType == MediaCoverTypes.Poster);
		}
	}
}
