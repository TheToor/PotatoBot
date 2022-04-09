using NLog;
using NLog.Targets;
using PotatoBot.Services;
using System;

namespace PotatoBot.Targets
{
    [Target("Telegram")]
    public sealed class TelegramTarget : TargetWithLayout
    {
        private readonly TelegramService _telegramService;

        public TelegramTarget(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        protected async override void Write(LogEventInfo logEvent)
        {
            try
            {
                var message = $"<b>[{logEvent.Level}][{logEvent.TimeStamp}]\n{logEvent.CallerClassName}->{logEvent.CallerMemberName}</b>\n{logEvent.Message}\n\n/debug_logfile";
                await _telegramService.SendToAdmin(message);
            }
            catch(Exception)
            {
                // Ignore it as only the log to Telegram failed not to file
            }
        }
    }
}
