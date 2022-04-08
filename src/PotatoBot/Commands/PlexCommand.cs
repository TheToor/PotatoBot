using PotatoBot.Modals;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("plex", Description = "Interacts with the Plex service")]
    internal class PlexCommand : Service, ICommand, IReplyCallback, IQueryCallback
    {
        public string UniqueIdentifier => "Plex";

        private const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            if(arguments.Length == 0)
            {
                // NO action
                await TelegramService.SimpleReplyToMessage(message, Program.LanguageManager.GetTranslation("Commands", "Plex", "Help"));
                return true;
            }

            var command = arguments[0];

            return command switch
            {
                "watch" => await HandleWatch(client, message, arguments),
                "invite" => await HandleInvite(message),
                _ => false,
            };
        }

        public async Task<bool> OnReplyReceived(TelegramBotClient client, Message message)
        {
            var email = message.Text;
            if(string.IsNullOrEmpty(email) || !Regex.IsMatch(email, EmailRegex, RegexOptions.IgnoreCase))
            {
                await TelegramService.SimpleReplyToMessage(message, Program.LanguageManager.GetTranslation("Commands", "Plex", "NotAnEmail"));
                return false;
            }

            var cacheData = TelegramService.GetCachedData<PlexData>(message);
            if(cacheData.Plex.Invite(email))
            {
                await TelegramService.SimpleReplyToMessage(
                    message,
                    string.Format(
                        Program.LanguageManager.GetTranslation("Commands", "Plex", "InviteSuccess"),
                        email
                    )
                );
            }
            else
            {
                await TelegramService.SimpleReplyToMessage(
                    message,
                    string.Format(
                        Program.LanguageManager.GetTranslation("Commands", "Plex", "InviteFail"),
                        email
                    )
                );
            }
            return true;
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            var selectedInstance = Program.ServiceManager.GetPlexServices().FirstOrDefault(p => p.Name == callbackQuery.Data);
            if(selectedInstance == null)
            {
                await TelegramService.SimpleReplyToMessage(callbackQuery.Message, Program.LanguageManager.GetTranslation("GeneralError"));
                return true;
            }
            var cacheData = TelegramService.GetCachedData<PlexData>(callbackQuery.Message);
            cacheData.Plex = selectedInstance;

            await TelegramService.ForceReply(this, callbackQuery.Message, Program.LanguageManager.GetTranslation("Commands", "Plex", "Invite"));
            return true;
        }

        private async Task<bool> HandleInvite(Message message)
        {
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            foreach(var plex in Program.ServiceManager.GetPlexServices())
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(plex.Name, plex.Name)
                });
            }

            var markup = new InlineKeyboardMarkup(keyboardMarkup);
            await TelegramService.ReplyWithMarkupAndData(
                this,
                message,
                Program.LanguageManager.GetTranslation("Commands", "Plex", "Selection"),
                markup,
                new PlexData()
            );
            return true;
        }

        private static async Task<bool> HandleWatch(TelegramBotClient client, Message message, string[] arguments)
        {
            await client.SendChatActionAsync(message.Chat!, Telegram.Bot.Types.Enums.ChatAction.Typing);

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

            Program.ServiceManager.WatchListService.AddToWatchList(message.From!.Id, service, item);
            await TelegramService.SimpleReplyToMessage(
                message,
                string.Format(
                    LanguageManager.GetTranslation("Commands", "Plex", "Watch"),
                    item.Title
                )
            );

            return true;
        }
    }
}
