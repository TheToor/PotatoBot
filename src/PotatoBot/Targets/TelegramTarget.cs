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
        
        private string _lastException = String.Empty;
        private DateTime _lastExceptionTime = DateTime.MinValue;

        public TelegramTarget(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        protected async override void Write(LogEventInfo logEvent)
        {
            try
            {
                if(_lastException == logEvent.Message && _lastExceptionTime.AddMinutes(1) > DateTime.Now)
                {
                    // Do not spam the same error message over and over
                    return;
                }

                _lastException = logEvent.Message;
                _lastExceptionTime = DateTime.Now;
                
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
