using PotatoBot.Model.API.Plex.Library;
using System.Collections.Generic;

namespace PotatoBot.Model.API.Plex
{
    public class RecentlyAddedResult
    {
        public List<Video> NewItems { get; set; } = new();
        public List<Library.Directory> NewDirectories { get; set; } = new();
    }
}
