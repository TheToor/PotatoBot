using PotatoBot.Model.Commands;
using PotatoBot.Model.Settings;
using PotatoBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PotatoBot.Commands
{
    [Command("debug", Description = "Debug command for admins")]
    public class DebugCommand : ICommand
    {
        private readonly TelegramService _telegramService;
        private readonly LogService _logService;
        private readonly BotSettings _botSettings;

        public DebugCommand(TelegramService telegramService, LogService logService, BotSettings botSettings)
        {
            _telegramService = telegramService;
            _logService = logService;
            _botSettings = botSettings;
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            if(!_telegramService.IsFromAdmin(message))
            {
                return true;
            }

            if(arguments.Length == 0)
            {
                // NO action
                return true;
            }

            var debugCommand = arguments[0].ToLower();

            switch(debugCommand)
            {
                case "log":
                {
                    if(arguments.Length != 2)
                    {
                        return true;
                    }

                    await HandleLogCommand(message, arguments);
                    break;
                }

                case "logfile":
                {
                    var currentLog = Path.Combine(_logService.LogDirectory, $"PotatoServer-{DateTime.Now.Date.ToString("yyyy-MM-dd")}.log");
                    if(System.IO.File.Exists(currentLog))
                    {
                        using var file = System.IO.File.Open(currentLog, FileMode.Open);
                        await client.SendDocumentAsync(message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(file, "TelegramBot.log"));
                    }
                    else
                    {
                        await _telegramService.SimpleReplyToMessage(message, "No log found");
                    }
                    break;
                }

                case "longmessage":
                {
                    var testMessage = "";
                    for(var i = 0; i < 256; i++)
                    {
                        testMessage += new string(i.ToString().Last(), 58);
                        testMessage += "\n";
                    }
                    await _telegramService.SendSimpleMessage(message.Chat!, testMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
                    break;
                }

                case "parametertest":
                {
                    var testMessage = "";
                    foreach(var argument in arguments)
                    {
                        testMessage += "Arg: " + argument;
                        testMessage += "\n";
                    }
                    await _telegramService.SendSimpleMessage(message.Chat!, testMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
                    break;
                }

                case "block":
                {
                    await _telegramService.SendSimpleMessage(message.Chat!, $"Starting wait on thread {Environment.CurrentManagedThreadId}", Telegram.Bot.Types.Enums.ParseMode.Html);
                    await Task.Delay(10000);
                    await _telegramService.SendSimpleMessage(message.Chat!, $"Done with block on thread {Environment.CurrentManagedThreadId}", Telegram.Bot.Types.Enums.ParseMode.Html);
                    break;
                }

                case "web":
                {
                    var button = new InlineKeyboardMarkup(InlineKeyboardButton.WithWebApp("Potato", new WebAppInfo()
                    {
                        Url = $"https://{_botSettings.Webhook.PublicUrl!}/Telegram/?data=test"
                    }));
                    await client.SendTextMessageAsync(message.Chat!, "DEBUG", replyMarkup: button);
                    break;
                }

                case "exception":
                {
                    throw new Exception("Debug exception");
                }
            }

            return true;
        }

        private async Task HandleLogCommand(Message message, string[] arguments)
        {
            var level = arguments[1];
            if(level == "0")
            {
                _logService.SetTelegramMinLogLevel(NLog.LogLevel.Error);
                await _telegramService.SimpleReplyToMessage(message, "Loglevel set to ERROR");
            }
            else if(level == "1")
            {
                _logService.SetTelegramMinLogLevel(NLog.LogLevel.Warn);
                await _telegramService.SimpleReplyToMessage(message, "Loglevel set to WARN");
            }
            else if(level == "2")
            {
                _logService.SetTelegramMinLogLevel(NLog.LogLevel.Info);
                await _telegramService.SimpleReplyToMessage(message, "Loglevel set to INFO");
            }
            else
            {
                await _telegramService.SimpleReplyToMessage(message, "Unsupported Loglevel (0 = Error, 1 = Warn, 2 = Info)");
            }
        }
    }
}
