using System;

namespace PotatoBot.Model.API.Lidarr
{
    public class Album
    {
        public ulong ArstidId { get; set; }

        public string Title { get; set; }
        public string Overview { get; set; }

        public bool Monitored { get; set; }
        public uint ProfileId { get; set; }
        public DateTime ReleaseDate { get; set; }

        public Artist Artist { get; set; }
    }
}
