using Newtonsoft.Json;
using PotatoBot.Managers;
using PotatoBot.Modals.Settings;
using System;
using System.IO;
using System.Threading;

namespace PotatoBot
{
    internal class Program
    {
        internal static BotSettings Settings;

        internal static LogManager LogManager;
        internal static ServiceManager ServiceManager;
        internal static LanguageManager LanguageManager;

        private static string _namespace;
        internal static string Namespace
        {
            get
            {
                if(string.IsNullOrEmpty(_namespace))
                {
                    _namespace = typeof(Program).Namespace;
                }
                return _namespace;
            }
        }

        private static Version _version;
        internal static Version Version
        {
            get
            {
                if(_version == null)
                {
                    _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                }
                return _version;
            }
        }

        private const string SettingsFileName = "settings.json";
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static volatile bool _exit;
        private static volatile bool _exited;

#if DEBUG
        internal static int TestMain(string[] args)
        {
            return Main(args);
        }
#endif

        private static int Main(string[] args)
        {
            try
            {
                LogManager = new LogManager();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to start LogManager: {ex}");
                return 1;
            }

            _logger.Info($"Initializing {Namespace} v{Version} ...");

            if(!ReadSettings())
            {
                return 2;
            }

            _logger.Trace("Starting LanguageManager ...");
            LanguageManager = new LanguageManager();

            _logger.Trace("Starting ServiceManager ...");
            ServiceManager = new ServiceManager();

            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
            Console.CancelKeyPress += ProcessExit;

            _logger.Info("Bot startup completed");

            do
            {
                Thread.Sleep(10);
            }
            while(!_exit);

            _logger.Trace("Stop request received");

            _logger.Trace("Stopping all services ...");
            ServiceManager.StopAllServices();

            _exited = true;

            _logger.Trace("All services stopped");

            NLog.LogManager.Flush();

            return 0;
        }

        internal static void ProcessExit(object sender, EventArgs e)
        {
            if(_exit || _exited)
            {
                return;
            }

            _exit = true;
            _logger.Info("Stop requested");

            do
            {
                Thread.Sleep(10);
            }
            while(!_exited);

            return;
        }

        private static bool ReadSettings()
        {
            _logger.Trace($"Reading Settings from file {SettingsFileName}");

            if(!File.Exists(SettingsFileName))
            {
                try
                {
                    Settings = new BotSettings();
                    SaveSettings();
                }
                catch(Exception ex)
                {
                    _logger.Fatal(ex, $"Failed to write settings file to {SettingsFileName}. Aborting execution");
                    return false;
                }
            }

            var settings = File.ReadAllText(SettingsFileName);
            if(string.IsNullOrEmpty(settings))
            {
                _logger.Fatal("Invalid settings file");
                return false;
            }

            try
            {
                Settings = JsonConvert.DeserializeObject<BotSettings>(settings);

#if TRACE
                _logger.Trace("===================================================================");
                _logger.Trace("Read Settings: ");
                _logger.Trace(JsonConvert.SerializeObject(Settings));
                _logger.Trace("===================================================================");
#endif
            }
            catch(Exception ex)
            {
                _logger.Fatal(ex, "Invalid settings file contents");
                return false;
            }

            _logger.Info("Successfully read settings");
            return true;
        }

        internal static bool SaveSettings()
        {
            _logger.Trace($"Saving Settings to file {SettingsFileName}");

            try
            {
                if(File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }

                File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(Settings));

                _logger.Info("Successfully saved settings");

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to save settings");
                return false;
            }
        }
    }
}
