using NLog;
using NLog.Targets;
using System;

namespace PotatoBot.Targets
{
    [Target("Telegram")]
    public sealed class TelegramTarget : TargetWithLayout
    {
        public TelegramTarget()
        {
        }

        protected async override void Write(LogEventInfo logEvent)
        {
            try
            {
                var message = $"<b>[{logEvent.Level}][{logEvent.TimeStamp}]\n{logEvent.CallerClassName}->{logEvent.CallerMemberName}</b>\n{logEvent.Message}\n\n/debug_logfile";
                if(Program.ServiceManager == null || Program.ServiceManager.TelegramService == null)
                {
                    return;
                }
                await Program.ServiceManager.TelegramService.SendToAdmin(message);
            }
            catch(Exception)
            {
                // Ignore it as only the log to Telegram failed not to file
            }
        }
    }
}
