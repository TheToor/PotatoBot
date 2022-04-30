namespace PotatoBot.Model.API.Requests
{
    public class RequestEpisode : RequestBase
    {
        public ulong SeriesId { get; set; }

        public override string ToGet()
        {
            return $"seriesId={SeriesId}";
        }
    }
}
