using ByteSizeLib;
using PotatoBot.API;
using PotatoBot.Modals.API;
using PotatoBot.Modals.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("storage", Description = "Show free storage")]
    internal class StorageCommand : Service, ICommand
    {
        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {           
            var services = new Dictionary<string, APIBase>();
            if(Program.Settings.Radarr.Enabled)
            {
                services.Add("Radarr", Program.ServiceManager.Radarr);
            }
            if(Program.Settings.Sonarr.Enabled)
            {
                services.Add("Sonarr", Program.ServiceManager.Sonarr);
            }
            if(Program.Settings.Lidarr.Enabled)
            {
                services.Add("Lidarr", Program.ServiceManager.Lidarr);
            }

            var responseText = string.Format(
                LanguageManager.GetTranslation("Commands", "Storage", "Storage"),
                services.Count
            );

            foreach (var service in services)
            {
                var disks = service.Value.GetDiskSpace();
                if (disks != null)
                {
                    responseText += $"<b>{service.Key}</b>\n";
                    responseText += disks
                        .Select(
                            (i) => {
                                var freeSpace = ByteSize.FromBytes(i.FreeSpace);
                                var totalSpace = ByteSize.FromBytes(i.TotalSpace);

                                return string.Format(
                                    LanguageManager.GetTranslation("Commands", "Storage", "Label"),
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

            await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }
    }
}
