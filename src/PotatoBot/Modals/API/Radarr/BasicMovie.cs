using System;
using System.Linq;

namespace PotatoBot.Modals.API.Radarr
{
    public class BasicMovie
	{
		public string Title { get; set; }
		public long SizeOnDisk { get; set; }
		public Image Poster { get; set; }
		public string Studio { get; set; }
		public ushort Runtime { get; set; }

		public BasicMovie(IServarrItem item)
		{
			var movie = item as Movie ?? throw new ArgumentNullException(nameof(item));

			Title = movie.Title;
			SizeOnDisk = movie.SizeOnDisk;
			Poster = movie.Images?.FirstOrDefault((i) => i.CoverType == MediaCoverTypes.Poster);
			Studio = movie.Studio;
			Runtime = movie.Runtime;
		}
	}
}
