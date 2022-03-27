using PotatoBot.Modals.Commands.FormatProviders;
using System.Collections.Generic;

namespace PotatoBot.Modals.Commands.Data
{
    public class SearchData : IData
    {
        public ServarrType SelectedSearch { get; set; }
        public string SearchText { get; set; }
        public IEnumerable<IServarr> API { get; set; }

        public IEnumerable<IServarrItem> SearchResults { get; set; }

        public ISearchFormatProvider SearchFormatProvider { get; set; }
    }
}
