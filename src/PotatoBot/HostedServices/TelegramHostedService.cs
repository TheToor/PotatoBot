using Microsoft.Extensions.Hosting;
using PotatoBot.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoBot.HostedServices
{
    public class TelegramHostedService : IHostedService
    {
        private readonly TelegramService _telegramService;
        private readonly CommandService _commandManager;

        public TelegramHostedService(TelegramService telegramService, CommandService commandManager)
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
