using PotatoBot.Services;

namespace PotatoBot.Modals.Commands.Data
{
    public class PlexData : IData
    {
        public PlexService Plex { get; set; }
    }
}
