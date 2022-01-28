using PotatoBot.Modals;
using PotatoBot.Modals.Commands;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("plex", Description = "Interacts with the Plex service")]
    internal class PlexCommand : Service, ICommand
    {
        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            if(arguments.Length == 0)
            {
                // NO action
                return true;
            }

            var command = arguments[0];

            switch(command)
            {
                case "watch":
                {
                    await client.SendChatActionAsync(message.Chat, Telegram.Bot.Types.Enums.ChatAction.Typing);

                    if(arguments.Length != 3)
                    {
                        return false;
                    }

                    var serviceName = arguments[1];
                    var id = ulong.Parse(arguments[2]);

                    if(Program.ServiceManager.GetAllServices().FirstOrDefault(s => s is IServarr && s.Name == serviceName) is not IServarr service)
                    {
                        return false;
                    }

                    var item = service.GetById(id);
                    if(item == null)
                    {
                        return false;
                    }

                    Program.ServiceManager.WatchListService.AddToWatchList(message.From.Id, service, item);
                    await TelegramService.SimpleReplyToMessage(
                        message,
                        string.Format(
                            LanguageManager.GetTranslation("Commands", "Plex", "Watch"),
                            item.Title
                        )
                    );

                    break;
                }
            }

            return true;
        }
    }
}
