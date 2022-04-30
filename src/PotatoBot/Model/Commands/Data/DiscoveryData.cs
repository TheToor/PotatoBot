using PotatoBot.Model.Commands.FormatProviders;
using System.Collections.Generic;

namespace PotatoBot.Model.Commands.Data
{
    public class DiscoveryData : IData, IProvidesSearch
    {
        public ServarrType SelectedSearch { get; set; }
        public IServarr? API { get; set; }

        public IEnumerable<IServarrItem>? SearchResults { get; set; }

        public ISearchFormatProvider SearchFormatProvider { get; set; }

        public DiscoveryData(ISearchFormatProvider searchFormatProvider)
        {
            SelectedSearch = ServarrType.Unknown;
            SearchFormatProvider = searchFormatProvider;
        }
    }
}
