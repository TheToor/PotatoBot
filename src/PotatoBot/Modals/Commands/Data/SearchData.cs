using PotatoBot.API;
using PotatoBot.Modals.API.Lidarr;
using PotatoBot.Modals.API.Radarr;
using PotatoBot.Modals.API.Sonarr;
using System.Collections.Generic;

namespace PotatoBot.Modals.Commands.Data
{
    public class SearchData : IData
    {
        public SearchType SelectedSearch { get; set; }
        public string SearchText { get; set; }
        public APIBase API { get; set; }

        public List<Series> SeriesSearchResults { get; set; }
        public List<Movie> MovieSearchResults { get; set; }
        public List<Artist> ArtistSearchResults { get; set; }
    }
}
