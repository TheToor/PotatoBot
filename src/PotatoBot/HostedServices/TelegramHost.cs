using Microsoft.Extensions.Hosting;
using PotatoBot.Managers;
using PotatoBot.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoBot.HostedServices
{
    public class TelegramHost : IHostedService
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TelegramService _telegramService;
        private readonly CommandManager _commandManager;

        public TelegramHost(TelegramService telegramService, CommandManager commandManager)
        {
            _telegramService = telegramService;
            _commandManager = commandManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _commandManager.LoadCommands();
            await _telegramService.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _telegramService.StopAsync(cancellationToken);
        }
    }
}
