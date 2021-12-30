using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;
using System.Reflection;

namespace PotatoBot.Managers
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
                var message = $"*[{logEvent.Level}][{logEvent.TimeStamp}]\n{logEvent.CallerClassName}->{logEvent.CallerMemberName}*\n{logEvent.Message}\n\n/debug_logfile";
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

    internal class LogManager
    {
        private string _logDirectory;
        public string LogDirectory
        {
            get
            {
                if(string.IsNullOrEmpty(_logDirectory))
                {
                    _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                }
                return _logDirectory;
            }
        }
        public string LogFileName { get; } = "log.config";

        public string LogPath => Path.Combine(Directory.GetCurrentDirectory(), LogFileName);

        private static LoggingConfiguration _configuration;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal LogManager()
        {
            InitializeLogging();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal((Exception)e.ExceptionObject, "Unhandeled exception");
        }

        internal void InitializeLogging()
        {
            if(!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            if(!File.Exists(LogFileName))
            {
                // Export log config from embedded resource

                var assembly = Assembly.GetExecutingAssembly();
                using(var resource = assembly.GetManifestResourceStream($"{Program.Namespace}.Resources.logconfig.xml"))
                {
                    using(var file = new StreamReader(resource))
                    {
                        File.WriteAllText(LogFileName, file.ReadToEnd());
                    }
                }
            }

            _configuration = new XmlLoggingConfiguration(LogFileName);

            foreach(var target in _configuration.AllTargets)
            {
                if(target is AsyncTargetWrapper wrapper)
                {
                    if(wrapper.WrappedTarget is not FileTarget fileTarget)
                    {
                        continue;
                    }

                    fileTarget.FileName = Path.Combine(LogDirectory,
                        fileTarget.FileName
                            .ToString()
                            .Trim('\'')
                    );
                }
            }

            var telegramTarget = new TelegramTarget();
            _configuration.AddTarget("Telegram", telegramTarget);
            _configuration.AddRule(LogLevel.Error, LogLevel.Fatal, "Telegram");

            NLog.LogManager.Configuration = _configuration;
            NLog.LogManager.ReconfigExistingLoggers();
            NLog.LogManager.EnableLogging();
        }

        internal static void SetTelegramMinLogLevel(LogLevel logLevel)
        {
            _configuration.RemoveRuleByName("Telegram");
            _configuration.AddRule(logLevel, LogLevel.Fatal, "Telegram");

            NLog.LogManager.Configuration = _configuration;
            NLog.LogManager.ReconfigExistingLoggers();
        }
    }
}
