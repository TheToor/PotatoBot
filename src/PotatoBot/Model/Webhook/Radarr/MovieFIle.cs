namespace PotatoBot.Model.Webhook.Radarr
{
    public class MovieFile
    {
        public int Id { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
    }
}
