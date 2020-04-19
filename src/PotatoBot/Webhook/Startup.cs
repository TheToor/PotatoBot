using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace PotatoBot.Webhook
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc((options) =>
            {
                // required for app.UseMvc() to work
                options.EnableEndpointRouting = false;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
