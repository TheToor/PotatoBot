using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.IO;
using System.Reflection;

namespace PotatoBot.Managers
{
    internal class LogManager
    {
        private string _logDirectory;
        public string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_logDirectory))
                {
                    _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                }
                return _logDirectory;
            }
        }
        public string LogFileName { get; } = "log.config";

        public string LogPath => Path.Combine(Directory.GetCurrentDirectory(), LogFileName);

        private static LoggingConfiguration _configuration;

        internal LogManager()
        {
            InitializeLogging();
        }

        internal void InitializeLogging()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            if (!File.Exists(LogFileName))
            {
                // Export log config from embedded resource

                var assembly = Assembly.GetExecutingAssembly();
                using (var resource = assembly.GetManifestResourceStream($"{Program.Namespace}.Resources.logconfig.xml"))
                {
                    using (var file = new StreamReader(resource))
                    {
                        File.WriteAllText(LogFileName, file.ReadToEnd());
                    }
                }
            }

            _configuration = new XmlLoggingConfiguration(LogFileName);

            foreach (var target in _configuration.AllTargets)
            {
                if (target is AsyncTargetWrapper)
                {
                    if (!(((AsyncTargetWrapper)target).WrappedTarget is FileTarget fileTarget))
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

            NLog.LogManager.Configuration = _configuration;
            NLog.LogManager.ReconfigExistingLoggers();
            NLog.LogManager.EnableLogging();
        }
    }
}
