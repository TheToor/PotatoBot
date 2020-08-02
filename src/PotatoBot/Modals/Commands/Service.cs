using PotatoBot.Managers;
using PotatoBot.Services;

namespace PotatoBot.Modals.Commands
{
    internal class Service
    {
        internal static LanguageManager LanguageManager => Program.LanguageManager;
        internal static TelegramService TelegramService => Program.ServiceManager.TelegramService;
        internal static SonarrService SonarrService => Program.ServiceManager.Sonarr;
        internal static RadarrService RadarrService => Program.ServiceManager.Radarr;
        internal static LidarrService LidarrService => Program.ServiceManager.Lidarr;
        internal static StatisticsService StatisticsService => Program.ServiceManager.StatisticsService;
    }
}
