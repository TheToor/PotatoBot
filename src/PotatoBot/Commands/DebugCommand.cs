using PotatoBot.Managers;
using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
	[Command("debug", Description = "Debug command for admins")]
	internal class DebugCommand : Service, ICommand
	{
		public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
		{
			if(!TelegramService.IsFromAdmin(message))
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

					var level = arguments[1];

					if(level == "0")
					{
						LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Error);
						await TelegramService.SimpleReplyToMessage(message, "Loglevel set to ERROR");
					}
					else if(level == "1")
					{
						LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Warn);
						await TelegramService.SimpleReplyToMessage(message, "Loglevel set to WARN");
					}
					else if(level == "2")
					{
						LogManager.SetTelegramMinLogLevel(NLog.LogLevel.Info);
						await TelegramService.SimpleReplyToMessage(message, "Loglevel set to INFO");
					}
					else
					{
						await TelegramService.SimpleReplyToMessage(message, "Unsupported Loglevel (0 = Error, 1 = Warn, 2 = Info)");
					}
					break;
				}

				case "logfile":
				{
					var currentLog = Path.Combine(Program.LogManager.LogDirectory, $"PotatoServer-{DateTime.Now.Date.ToString("yyyy-MM-dd")}.log");
					if(System.IO.File.Exists(currentLog))
					{
						using var file = System.IO.File.Open(currentLog, FileMode.Open);
						await client.SendDocumentAsync(message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(file, "TelegramBot.log"));
					}
					else
					{
						await TelegramService.SimpleReplyToMessage(message, "No log found");
					}
					break;
				}

				case "longmessage":
				{
					var testMessage = "";
					for(int i = 0; i < 256; i++)
					{
						testMessage += new string(i.ToString().Last(), 58);
						testMessage += "\n";
					}
					await TelegramService.SendSimpleMessage(message.Chat, testMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
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
					await TelegramService.SendSimpleMessage(message.Chat, testMessage, Telegram.Bot.Types.Enums.ParseMode.Html);
					break;
				}
			}

			return true;
		}
	}
}
