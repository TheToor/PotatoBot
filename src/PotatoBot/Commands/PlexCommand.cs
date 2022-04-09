using PotatoBot.HostedServices;
using PotatoBot.Managers;
using PotatoBot.Modals;
using PotatoBot.Modals.Commands;
using PotatoBot.Modals.Commands.Data;
using PotatoBot.Services;
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
    public class PlexCommand : ICommand, IReplyCallback, IQueryCallback
    {
        public string UniqueIdentifier => "Plex";

        private const string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        private readonly TelegramService _telegramService;
        private readonly ServiceManager _serviceManager;
        private readonly LanguageManager _languageManager;
        private readonly WatchListService _watchListService;

        public PlexCommand(TelegramService telegramService, ServiceManager serviceManager, LanguageManager languageManager, WatchListService watchListService)
        {
            _telegramService = telegramService;
            _serviceManager = serviceManager;
            _languageManager = languageManager;
            _watchListService = watchListService;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            if(arguments.Length == 0)
            {
                // NO action
                await _telegramService.SimpleReplyToMessage(message, _languageManager.GetTranslation("Commands", "Plex", "Help"));
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
                await _telegramService.SimpleReplyToMessage(message, _languageManager.GetTranslation("Commands", "Plex", "NotAnEmail"));
                return false;
            }

            var cacheData = _telegramService.GetCachedData<PlexData>(message);
            if(cacheData.Plex.Invite(email))
            {
                await _telegramService.SimpleReplyToMessage(
                    message,
                    string.Format(
                        _languageManager.GetTranslation("Commands", "Plex", "InviteSuccess"),
                        email
                    )
                );
            }
            else
            {
                await _telegramService.SimpleReplyToMessage(
                    message,
                    string.Format(
                        _languageManager.GetTranslation("Commands", "Plex", "InviteFail"),
                        email
                    )
                );
            }
            return true;
        }

        public async Task<bool> OnCallbackQueryReceived(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            var selectedInstance = _serviceManager.GetPlexServices().FirstOrDefault(p => p.Name == callbackQuery.Data);
            if(selectedInstance == null)
            {
                await _telegramService.SimpleReplyToMessage(callbackQuery.Message, _languageManager.GetTranslation("GeneralError"));
                return true;
            }
            var cacheData = _telegramService.GetCachedData<PlexData>(callbackQuery.Message);
            cacheData.Plex = selectedInstance;

            await _telegramService.ForceReply(this, callbackQuery.Message, _languageManager.GetTranslation("Commands", "Plex", "Invite"));
            return true;
        }

        private async Task<bool> HandleInvite(Message message)
        {
            var keyboardMarkup = new List<List<InlineKeyboardButton>>();
            foreach(var plex in _serviceManager.GetPlexServices())
            {
                keyboardMarkup.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(plex.Name, plex.Name)
                });
            }

            var markup = new InlineKeyboardMarkup(keyboardMarkup);
            await _telegramService.ReplyWithMarkupAndData(
                this,
                message,
                _languageManager.GetTranslation("Commands", "Plex", "Selection"),
                markup,
                new PlexData()
            );
            return true;
        }

        private async Task<bool> HandleWatch(TelegramBotClient client, Message message, string[] arguments)
        {
            await client.SendChatActionAsync(message.Chat!, Telegram.Bot.Types.Enums.ChatAction.Typing);

            if(arguments.Length != 3)
            {
                return false;
            }

            var serviceName = arguments[1];
            var id = ulong.Parse(arguments[2]);

            if(_serviceManager.GetAllServices().FirstOrDefault(s => s is IServarr && s.Name == serviceName) is not IServarr service)
            {
                return false;
            }

            var item = service.GetById(id);
            if(item == null)
            {
                return false;
            }

            _watchListService.AddToWatchList(message.From!.Id, service, item);
            await _telegramService.SimpleReplyToMessage(
                message,
                string.Format(
                    _languageManager.GetTranslation("Commands", "Plex", "Watch"),
                    item.Title
                )
            );

            return true;
        }
    }
}
