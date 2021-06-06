using System.Collections.Generic;

namespace PotatoBot.Modals.Commands.Data
{
    public class SearchData : IData
    {
        public SearchType SelectedSearch { get; set; }
        public string SearchText { get; set; }
        public IServarr API { get; set; }

        public IEnumerable<IServarrItem> SearchResults { get; set; }
    }
}
