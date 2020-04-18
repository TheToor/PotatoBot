namespace PotatoBot.Modals.API.Requests
{
    public class QueueRequest : RequestBase
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public string SortDirection { get; set; } = "ascending";
        public string SortKey { get; set; } = "sortKey";
        public bool IncludeUnknownSeriesItems { get; set; } = false;

        public override string ToGet()
        {
            return $"page={Page}&pageSize={PageSize}&sortDirection={SortDirection}&sortKey={SortKey}&includeUnknownSeriesItems={IncludeUnknownSeriesItems}";
        }
    }
}
