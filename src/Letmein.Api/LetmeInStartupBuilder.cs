using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Letmein.Api
{
    public class LetmeInStartupBuilder
	{
		public static WebApplication Configure(WebApplicationBuilder builder)
		{
            ConfigureSerilog(builder);
            ConfigureServices(builder.Services, builder.Configuration);

            var webApp = builder.Build();
            ConfigureWebApplication(webApp, webApp.Environment);

            return webApp;
        }
        private static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .Enrich.FromLogContext()
                    .WriteTo.Console(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [{SourceContext:l}] {Message}{NewLine}{Exception}")
                    .MinimumLevel.Information();

                loggerConfig.Enrich.FromLogContext();
            });
        }

        private static void ConfigureServices(IServiceCollection services, IConfigurationRoot configRoot)
        {
            services.AddLogging();
            services.AddHttpLogging(_ => { });
            services.AddRouting();
            services.AddControllers();

            // Add API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Letmein API",
                    Version = "v1",
                    Description = "A RESTful API for secure encrypted text sharing"
                });
            });

            DependencyInjection.ConfigureServices(services, configRoot);
        }

        private static void ConfigureWebApplication(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpLogging();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Letmein API v1");
                });
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
