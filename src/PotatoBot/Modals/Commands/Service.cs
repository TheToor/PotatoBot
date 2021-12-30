using PotatoBot.Managers;
using PotatoBot.Services;
using System.Collections.Generic;

namespace PotatoBot.Modals.Commands
{
    internal class Service
	{
		internal static LanguageManager LanguageManager => Program.LanguageManager;
		internal static TelegramService TelegramService => Program.ServiceManager.TelegramService;
		internal static List<SonarrService> SonarrService => Program.ServiceManager.Sonarr;
		internal static List<RadarrService> RadarrService => Program.ServiceManager.Radarr;
		internal static List<LidarrService> LidarrService => Program.ServiceManager.Lidarr;
		internal static StatisticsService StatisticsService => Program.ServiceManager.StatisticsService;
	}
}
