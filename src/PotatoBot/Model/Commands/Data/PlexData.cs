using PotatoBot.Services;

namespace PotatoBot.Model.Commands.Data
{
    public class PlexData : IData
    {
        public PlexService Plex { get; set; }
    }
}
