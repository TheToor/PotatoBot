﻿using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using PotatoBot.Managers;
using PotatoBot.Modals.Settings;
using System;
using System.IO;
using System.Threading;

namespace PotatoBot
{
    class Program
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
                if (string.IsNullOrEmpty(_namespace))
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
                if (_version == null)
                {
                    _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                }
                return _version;
            }
        }


        private const string _settingsFileName = "settings.json";
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
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

            Console.ReadLine();

            _logger.Info("Stop requested");

            return 0;
        }

        private static bool ReadSettings()
        {
            _logger.Trace($"Reading Settings from file {_settingsFileName}");

            if(!File.Exists(_settingsFileName))
            {
                try
                {
                    Settings = new BotSettings();
                    SaveSettings();
                }
                catch(Exception ex)
                {
                    _logger.Fatal(ex, $"Failed to write settings file to {_settingsFileName}. Aborting execution");
                    return false;
                }
            }

            var settings = File.ReadAllText(_settingsFileName);
            if(string.IsNullOrEmpty(settings))
            {
                _logger.Fatal("Invalid settings file");
                return false;
            }

            try
            {
                Settings = JsonConvert.DeserializeObject<BotSettings>(settings);
            }
            catch(Exception ex)
            {
                _logger.Fatal(ex, "Invalid settings file contents");
                return false;
            }

            _logger.Info("Successfully read settings");
            return true;
        }

        private static bool SaveSettings()
        {
            _logger.Trace($"Saving Settings to file {_settingsFileName}");

            try
            {
                if(File.Exists(_settingsFileName))
                {
                    File.Delete(_settingsFileName);
                }

                File.WriteAllText(_settingsFileName, JsonConvert.SerializeObject(Settings));

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
