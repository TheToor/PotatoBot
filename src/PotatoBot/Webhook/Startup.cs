using LettuceEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PotatoBot.Webhook
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors((options) =>
            {
                options.AddDefaultPolicy((builder) =>
                {
                    builder
                        .WithOrigins(Program.Settings.CORSUrls.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddLettuceEncrypt();

            services.AddMvc((options) =>
            {
                // required for app.UseMvc() to work
                options.EnableEndpointRouting = false;
            });

            services.Configure<JsonOptions>((option) =>
            {
                option.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<LettuceEncryptOptions>((options) =>
            {
                var settings = Program.Settings.LettuceEncrypt;
                options.AcceptTermsOfService = settings.AcceptTOS;
                options.DomainNames = settings.DomainNames.ToArray();
                options.EmailAddress = settings.Email;
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseMvc();
        }
    }
}
