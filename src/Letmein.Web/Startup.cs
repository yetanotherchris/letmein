using System.Security.Cryptography;
using Letmein.Core.Encryption;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.Postgres;
using Letmein.Core.Services;
using Letmein.Core.Services.UniqueId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StructureMap;

namespace Letmein.Web
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			// Setup Sirilog
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.LiterateConsole(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [Website] {Message}{NewLine}{Exception}")
				.CreateLogger();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string connectionString = Configuration["POSTGRES_CONNECTIONSTRING"];

			// Add framework services.
			services.AddMvc();
			services.AddLogging();

			services.AddScoped<SymmetricAlgorithm>(sp => Aes.Create());
			services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();
			services.AddScoped<ISymmetricEncryptionProvider, SymmetricEncryptionProvider>();
			services.AddScoped<ITextRepository>(sp => new TextRepository(connectionString));
			services.AddScoped<ITextEncryptionService, TextEncryptionService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddSerilog();
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "Index",
					template: "",
					defaults: new { controller = "Home", action = "Index" }
				);

				routes.MapRoute(
					name: "FAQ",
					template: "faq",
					defaults: new { controller = "Home", action = "FAQ" }
				);

				routes.MapRoute(
					name: "Store",
					template: "store",
					defaults: new { controller = "Home", action = "Store" }
				);

				routes.MapRoute(
					name: "Load",
					template: "load",
					defaults: new { controller = "Home", action = "Load" }
				);

				routes.MapRoute(
					name: "Load-Id",
					template: "{friendlyId}",
					defaults: new { controller = "Home", action = "Load" }
				);
			});
		}

		public static class IoC
		{
			public static Container Container { get; set; }
			static IoC()
			{
				Container = new Container();
			}
		}
	}
}
