using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Web;
using PotatoBot.HostedServices;
using PotatoBot.Model.Settings;
using PotatoBot.Services;
using System;
using System.IO;

namespace PotatoBot
{
    internal class Program
    {
        private static string? _namespace;
        internal static string Namespace
        {
            get
            {
                if(string.IsNullOrEmpty(_namespace))
                {
                    _namespace = typeof(Program).Namespace!;
                }
                return _namespace;
            }
        }

        private static Version? _version;
        internal static Version Version
        {
            get
            {
                if(_version == null)
                {
                    _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
                }
                return _version;
            }
        }

        private const string SettingsFileName = "settings.json";
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static IWebHostBuilder CreateHostBuilder()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var botSettings = ReadSettings();

            return new WebHostBuilder()
                .UseKestrel(kestrel =>
                {
                    // var appServices = kestrel.ApplicationServices;
                    // kestrel.ConfigureHttpsDefaults(options =>
                    // {
                    //     options.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
                    //     options.UseLettuceEncrypt(appServices);
                    // });
                })
                .UseConfiguration(config)
                .UseDefaultServiceProvider(cfg => { })
                .ConfigureLogging((logger) =>
                {
                    // Remove the default logger
                    logger.ClearProviders();
                    // Add NLog logger
                    logger.AddNLog(@"log.config");
                })
                .ConfigureServices((hostcontext, services) =>
                {
                    // services
                    //     .AddLettuceEncrypt()
                    //     .PersistDataToDirectory(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Cert")), botSettings.Telegram.BotToken);

                    services.AddSingleton(botSettings);
                    services.AddSingleton<LogService>();
                    services.AddSingleton<LanguageService>();
                    services.AddSingleton<StatisticsService>();
                    services.AddSingleton<ServiceManager>();
                    services.AddSingleton<TelegramService>();
                    services.AddSingleton<WatchListHostedService>();

                    services.AddSingleton<CommandService>();

                    services.AddHostedService<TelegramHostedService>();
                    services.AddHostedService<WebhookCacheHostedService>();
                    services.AddHostedService(serviceProvider => serviceProvider.GetService<WatchListHostedService>());

                    services.AddCors((options) =>
                    {
                        options.AddDefaultPolicy((builder) =>
                        {
                            builder
                                .WithOrigins(botSettings.CORSUrls.ToArray())
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                    });

                    services.AddMvc((options) =>
                    {
                        // required for app.UseMvc() to work
                        options.EnableEndpointRouting = false;
                    });
                    services.AddControllersWithViews();

                    services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>((option) =>
                    {
                        option.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    });
                    services.Configure<KestrelServerOptions>(options =>
                    {
                        options.AllowSynchronousIO = true;
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseCors();
                    app.UseMvc();
                    app.UseMvcWithDefaultRoute();
                    app.UseStaticFiles();
                    // app.UseHttpsRedirection();
                })
                .UseNLog()
                .UseUrls(botSettings.Webhook.BindingUrl!)
                .SuppressStatusMessages(true);
        }

#if DEBUG
        public static IWebHost TestMain()
        {
            var host = CreateHostBuilder().Build();
            return host;
        }
#endif

        public static int Main()
        {
            CreateHostBuilder().Build().Run();
            return 0;
        }

        private static BotSettings ReadSettings()
        {
            _logger.Trace($"Reading Settings from file {SettingsFileName}");

            if(!File.Exists(SettingsFileName))
            {
                var newSettings = new BotSettings();
                SaveSettings(newSettings);
                return newSettings;
            }

            var settingsContent = File.ReadAllText(SettingsFileName);
            if(string.IsNullOrEmpty(settingsContent))
            {
                throw new InvalidDataException("Invalid settings file");
            }

            var settings = JsonConvert.DeserializeObject<BotSettings>(settingsContent);
            if(settings == null)
            {
                throw new InvalidDataException("Invalid settings file");
            }
#if TRACE
            _logger.Trace("===================================================================");
            _logger.Trace("Read Settings: ");
            _logger.Trace(JsonConvert.SerializeObject(settings));
            _logger.Trace("===================================================================");
#endif

            _logger.Info("Successfully read settings");
            return settings;
        }

        internal static bool SaveSettings(BotSettings settings)
        {
            _logger.Trace($"Saving Settings to file {SettingsFileName}");

            try
            {
                if(File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }

                File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(settings));

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
