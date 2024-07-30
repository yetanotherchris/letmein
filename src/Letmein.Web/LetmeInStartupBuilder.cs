using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Letmein.Web
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
            services.AddControllersWithViews();
            services.AddRazorPages();

            DependencyInjection.ConfigureServices(services, configRoot);
        }

        private static void ConfigureWebApplication(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpLogging();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Index",
                    pattern: "",
                    defaults: new { controller = "Home", action = "Index" }
                );

                endpoints.MapControllerRoute(
                    name: "FAQ",
                    pattern: "faq",
                    defaults: new { controller = "Home", action = "FAQ" }
                );

                endpoints.MapControllerRoute(
                    name: "Store",
                    pattern: "store",
                    defaults: new { controller = "Home", action = "Store" }
                );

                endpoints.MapControllerRoute(
                    name: "Load",
                    pattern: "load",
                    defaults: new { controller = "Home", action = "Load" }
                );

                endpoints.MapControllerRoute(
                    name: "Delete",
                    pattern: "delete",
                    defaults: new { controller = "Home", action = "Delete" }
                );

                endpoints.MapControllerRoute(
                    name: "Load-Id",
                    pattern: "{friendlyId}",
                    defaults: new { controller = "Home", action = "Load" }
                );

                endpoints.MapRazorPages();
            });
        }
    }
}