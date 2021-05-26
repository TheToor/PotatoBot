namespace PotatoBot.Modals.Settings
{
    public class EntertainmentSettings
    {
        public bool Enabled { get; set; }
        public bool EnableCalendar { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        public string APIKey { get; set; }

        public string CalendarOptions { get; set; }

        public string DownloadPath { get; set; }
        public uint QualityProfile { get; set; }
        public uint LanguageProfile { get; set; }
    }
}
