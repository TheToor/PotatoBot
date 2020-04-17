using PotatoBot.Managers;
using PotatoBot.Services;

namespace PotatoBot.Modals.Commands
{
    internal class Service
    {
        internal LanguageManager LanguageManager => Program.LanguageManager;
        internal TelegramService TelegramService => Program.ServiceManager.TelegramService;
        internal SonarrService SonarrService => Program.ServiceManager.Sonarr;
    }
}
