namespace PotatoBot.Model.Webhook.Lidarr
{
    public class Release
    {
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
