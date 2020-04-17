namespace PotatoBot.Modals.Settings
{
    public class EntertainmentSettings
    {
        public bool Enabled { get; set; }

        public string Url { get; set; }
        public string APIKey { get; set; }

        public string CalenderOptions { get; set; }

        public string DownloadPath { get; set; }
        public string DefaultProfile { get; set; }
    }
}
