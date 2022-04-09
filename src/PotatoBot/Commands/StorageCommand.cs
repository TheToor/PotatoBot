using ByteSizeLib;
using PotatoBot.Controllers;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Settings;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("storage", Description = "Show free storage")]
    public class StorageCommand : ICommand
    {
        private readonly TelegramService _telegramService;
        private readonly LanguageService _languageManager;
        private readonly ServiceManager _serviceManager;
        private readonly BotSettings _settings;

        public StorageCommand(TelegramService telegramService, LanguageService languageManager, ServiceManager serviceManager, BotSettings settings)
        {
            _telegramService = telegramService;
            _languageManager = languageManager;
            _serviceManager = serviceManager;
            _settings = settings;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            var services = new Dictionary<string, APIBase>();
            if(_settings.Radarr.Count > 0)
            {
                foreach(var service in _serviceManager.Radarr)
                {
                    services.Add(service.Name, service);
                }
            }
            if(_settings.Sonarr.Count > 0)
            {
                foreach(var service in _serviceManager.Sonarr)
                {
                    services.Add(service.Name, service);
                }
            }
            if(_settings.Lidarr.Count > 0)
            {
                foreach(var service in _serviceManager.Lidarr)
                {
                    services.Add(service.Name, service);
                }
            }

            var responseText = string.Format(
                _languageManager.GetTranslation("Commands", "Storage", "Storage"),
                services.Count
            );

            foreach(var service in services)
            {
                var disks = service.Value.GetDiskSpace();
                if(disks != null)
                {
                    responseText += $"<b>{service.Key}</b>\n";
                    responseText += disks
                        .Select(
                            (i) =>
                            {
                                var freeSpace = ByteSize.FromBytes(i.FreeSpace);
                                var totalSpace = ByteSize.FromBytes(i.TotalSpace);

                                return string.Format(
                                    _languageManager.GetTranslation("Commands", "Storage", "Label"),
                                    i.Path,
                                    i.Label,
                                    $"{(int)Math.Round(freeSpace.LargestWholeNumberBinaryValue)} {freeSpace.LargestWholeNumberBinarySymbol}",
                                    $"{(int)Math.Round(totalSpace.LargestWholeNumberBinaryValue)} {totalSpace.LargestWholeNumberBinarySymbol}"
                                );
                            }
                        )
                        .Aggregate((i, j) => i + "\n" + j);

                    responseText += "\n\n";
                }
            }

            await _telegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }
    }
}
