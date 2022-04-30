namespace PotatoBot.Model.API.Radarr
{
    public class AlternativeTitle
    {
        public string SourceType { get; set; }
        public uint MovieId { get; set; }
        public string Title { get; set; }
        public uint SourceId { get; set; }
        public uint Votes { get; set; }
        public uint VoteCount { get; set; }
        public string Language { get; set; }
        public uint Id { get; set; }
    }
}
