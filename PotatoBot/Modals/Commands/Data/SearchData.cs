using PotatoBot.Modals.API.Sonarr;
using System.Collections.Generic;

namespace PotatoBot.Modals.Commands.Data
{
    public class SearchData : IData
    {
        public SearchType SelectedSearch { get; set; }
        public List<Series> SeriesSearchResults { get; set; }
    }
}
