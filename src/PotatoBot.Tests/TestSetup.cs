using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PotatoBot.Services;
using System;
using System.Threading.Tasks;

namespace PotatoBot.Tests
{
    public class TestSetup : IAsyncDisposable, IDisposable
    {
        public readonly ServiceManager ServiceManager;

        private readonly IWebHost _host;

        public TestSetup()
        {
            _host = Program.TestMain();
            Task.Factory.StartNew(
                () => _host.Run(),
                TaskCreationOptions.LongRunning
            );

            do
            {
                ServiceManager = _host.Services.GetService<ServiceManager>()!;
            }
            while (ServiceManager == null);
        }

        public void Dispose()
        {
            _host.StopAsync().Wait();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();
            GC.SuppressFinalize(this);
        }
    }
}
