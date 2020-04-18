﻿using PotatoBot.Modals.Commands;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("debug")]
    internal class DebugCommand : Service, ICommand
    {
        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
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

                        var level = arguments[1];

                        if(level == "0")
                        {
                            Program.LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Error);
                            await TelegramService.SimpleReplyToMessage(message, "Loglevel set to ERROR");
                        }
                        else if(level == "1")
                        {
                            Program.LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Warn);
                            await TelegramService.SimpleReplyToMessage(message, "Loglevel set to WARN");
                        }
                        else if (level == "2")
                        {
                            Program.LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Info);
                            await TelegramService.SimpleReplyToMessage(message, "Loglevel set to INFO");
                        }
                        else
                        {
                            await TelegramService.SimpleReplyToMessage(message, "Unsupported Loglevel (0 = Error, 1 = Warn, 2 = Info)");
                        }
                    }
                    break;
            }

            return true;
        }
    }
}
