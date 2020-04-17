namespace PotatoBot.Modals.API.Requests
{
    public class LookupRequest : RequestBase
    {
        public string SearchTerm { get; set;
        }
        public override string ToGet()
        {
            return $"term={System.Web.HttpUtility.UrlEncode(SearchTerm)}";
        }
    }
}
