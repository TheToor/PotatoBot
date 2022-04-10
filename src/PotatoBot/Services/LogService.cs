using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using PotatoBot.Targets;
using System;
using System.IO;

namespace PotatoBot.Services
{
    public class LogService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string? _logDirectory;
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

        private LoggingConfiguration? _configuration;

        private readonly TelegramService _telegramService;

        public LogService(TelegramService telegramService)
        {
            _telegramService = telegramService;

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
                throw new FileNotFoundException(LogFileName);
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
                            .ToString()!
                            .Trim('\'')
                    );
                }
            }

            var telegramTarget = new TelegramTarget(_telegramService);
            _configuration.AddTarget("Telegram", telegramTarget);
            _configuration.AddRule(LogLevel.Error, LogLevel.Fatal, "Telegram");

            LogManager.Configuration = _configuration;
            LogManager.ReconfigExistingLoggers();
            LogManager.EnableLogging();
        }

        internal void SetTelegramMinLogLevel(LogLevel logLevel)
        {
            if(_configuration == null)
            {
                return;
            }

            _configuration.RemoveRuleByName("Telegram");
            _configuration.AddRule(logLevel, LogLevel.Fatal, "Telegram");

            LogManager.Configuration = _configuration;
            LogManager.ReconfigExistingLoggers();
        }
    }
}
