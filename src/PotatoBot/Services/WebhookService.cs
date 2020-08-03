﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Web;
using Org.BouncyCastle.Utilities.Net;
using PotatoBot.Webhook;
using System;
using System.Threading;

namespace PotatoBot.Services
{
    public class WebhookService : IService
    {
        public string Name => "Webhook Endpoint";

        private Modals.Settings.WebhookSettings _settings => Program.Settings.Webhook;

        private IWebHost _endpoint;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool Start()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .Build();

                _endpoint = new WebHostBuilder()
                    .UseConfiguration(config)
                    .UseStartup<Startup>()
                    .PreferHostingUrls(false)
                    .UseKestrel((kestrel) =>
                    {
                        var appServices = kestrel.ApplicationServices;
                        kestrel.ConfigureHttpsDefaults((httpsOptions) =>
                        {
                            httpsOptions.UseLettuceEncrypt(appServices);
                        });
                    })
                    .ConfigureLogging((logging) =>
                    {
                        logging.ClearProviders();
#if WEB_DEBUG
                        logging.SetMinimumLevel(LogLevel.Trace);
#else
                        logging.SetMinimumLevel(LogLevel.Warning);
#endif
                    })
                    .UseNLog()
                    .UseUrls(new[] { _settings.BindingUrl, _settings.HTTPSBindingUrl })
                    .SuppressStatusMessages(true)
                    .Build();

                _endpoint.RunAsync(_cancellationTokenSource.Token);

                _logger.Info($"Started {Name} on '{_settings.BindingUrl}'");
                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to start {Name}");
                return false;
            }
        }

        public bool Stop()
        {
            _logger.Info($"Received Stop signal for {Name}");

            try
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _logger.Trace("Requesting cancellation ...");
                    _cancellationTokenSource.Cancel();
                    _logger.Trace("Waiting for shutdown ...");
                    _endpoint.WaitForShutdown();
                    _logger.Trace("Cleaning up ...");
                    _endpoint.Dispose();
                }
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"Failed to correctly stop {Name}");
            }

            return true;
        }
    }
}
