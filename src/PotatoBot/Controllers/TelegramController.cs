using Microsoft.AspNetCore.Mvc;
using PotatoBot.Model;
using PotatoBot.Model.Settings;
using PotatoBot.Model.Webhook.Telegram;
using PotatoBot.Services;
using System;
using System.Linq;

namespace PotatoBot.Controllers
{
    public class TelegramController : Controller
    {
        private readonly BotSettings _botSettings;
        private readonly ServiceManager _serviceManager;

        public TelegramController(BotSettings botSettings, ServiceManager serviceManager)
        {
            _botSettings = botSettings;
            _serviceManager = serviceManager;
        }

        public IActionResult Index()
        {
            var model = new InitModel()
            {
                Radarr = _botSettings.Radarr.Where(s => s.Enabled),
                Sonarr = _botSettings.Sonarr.Where(s => s.Enabled),
                Lidarr = _botSettings.Lidarr.Where(s => s.Enabled)
            };
            return View(model);
        }

        public IActionResult Library(ServarrType type, string name)
        {
            IServarr instance = type switch
            {
                ServarrType.Radarr => _serviceManager.Radarr.First(s => s.Name == name),
                ServarrType.Sonarr => _serviceManager.Sonarr.First(s => s.Name == name),
                ServarrType.Lidarr => _serviceManager.Lidarr.First(s => s.Name == name),
                _ => throw new NotImplementedException()
            };

            return View(instance.GetQueue());
        }
    }
}
