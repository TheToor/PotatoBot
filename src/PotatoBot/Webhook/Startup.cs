using Microsoft.AspNetCore.Builder;
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
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
