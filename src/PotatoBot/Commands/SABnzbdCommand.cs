using PotatoBot.Managers;
using PotatoBot.Modals.Commands;
using PotatoBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PotatoBot.Commands
{
    [Command("sab", Description = "Controls SAB servers")]
    internal class SABnzbdCommand : Service, ICommand
    {
        internal enum SABnzbdCommandMode
        {
            Status,
            Pause,
            Resume
        }

        public async Task<bool> Execute(TelegramBotClient client, Message message, string[] arguments)
        {
            if(arguments.Length == 0 || !Enum.TryParse(arguments[0], true, out SABnzbdCommandMode command))
            {
                await TelegramService.SimpleReplyToMessage(
                    message,
                    LanguageManager.GetTranslation("Commands", "SABnzbd", "CommandNotFound")
                );
                return true;
            }

            var servers = Program.ServiceManager.GetSABnzbdServices();
            switch (command)
            {
                case SABnzbdCommandMode.Status:
                    return await ProcessStatusCommand(message, servers);

                case SABnzbdCommandMode.Pause:
                    return await ProcessPauseCommand(message, servers);

                case SABnzbdCommandMode.Resume:
                    return await ProcessResumeCommand(message, servers);
            }

            return true;
        }

        private static async Task<bool> ProcessStatusCommand(Message message, List<SABnzbdService> servers)
        {
            var responseText = string.Format(
                LanguageManager.GetTranslation("Commands", "SABnzbd", "Status", "Title"),
                servers.Count
            );

            foreach(var server in servers)
            {
                // Queue returns queue and fullstatus
                var response = await server.GetQueue();
                if(response == null)
                {
                    continue;
                }

                // response.Status if using server.GetStatus() !
                var status = response.Queue;

                responseText += $"<b>{server.Name}</b>\n";
                responseText += string.Format(
                    LanguageManager.GetTranslation("Commands", "SABnzbd", "Status", "Text"),
                    status.Paused ? status.Paused : status.PausedAll,
                    status.Version,
                    status.LoadAverage,
                    status.Diskspace1Norm,
                    status.SizeLeft,
                    status.TimeLeft,
                    status.EstimatedRemainingTime
                );
                responseText += "\n\n";
            }

            await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }

        private static async Task<bool> ProcessPauseCommand(Message message, List<SABnzbdService> servers)
        {
            var responseText = string.Format(
                LanguageManager.GetTranslation("Commands", "SABnzbd", "Pause"),
                servers.Count
            );

            foreach(var server in servers)
            {
                var response = await server.PauseQueue();

                responseText += $"{server.Name}: {response} \n";
            }

            await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }

        private static async Task<bool> ProcessResumeCommand(Message message, List<SABnzbdService> servers)
        {
            var responseText = string.Format(
                LanguageManager.GetTranslation("Commands", "SABnzbd", "Resume"),
                servers.Count
            );

            foreach (var server in servers)
            {
                var response = await server.ResumeQueue();

                responseText += $"{server.Name}: {response} \n";
            }

            await TelegramService.SimpleReplyToMessage(message, responseText, Telegram.Bot.Types.Enums.ParseMode.Html);

            return true;
        }
    }
}
