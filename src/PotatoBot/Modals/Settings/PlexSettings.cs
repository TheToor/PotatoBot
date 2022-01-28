using System.Collections.Generic;

namespace PotatoBot.Modals.Settings
{
    public class PlexSettings
    {
        public bool Enabled { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string APIKey { get; set; }

        public Dictionary<string, string> PathOverrides { get; set; } = new();
    }
}
