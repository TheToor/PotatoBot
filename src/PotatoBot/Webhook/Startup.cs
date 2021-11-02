using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

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

			services.AddMvc((options) =>
			{
				// required for app.UseMvc() to work
				options.EnableEndpointRouting = false;
			});
			services.Configure<JsonOptions>((option) =>
			{
				option.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
			});
			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
		}
		public void Configure(IApplicationBuilder app)
		{
			app.UseCors();

			app.UseMvc();
		}
	}
}
