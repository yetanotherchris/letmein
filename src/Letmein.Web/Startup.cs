using System.Security.Cryptography;
using Letmein.Core;
using Letmein.Core.Configuration;
using Letmein.Core.Encryption;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.Postgres;
using Letmein.Core.Services;
using Letmein.Core.Services.UniqueId;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StructureMap;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;

namespace Letmein.Web
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }

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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
			services.AddLogging();

			services.AddSingleton<IConfigurationRoot>(sp => Configuration);
			services.AddSingleton<IConfiguration>(sp => new DefaultConfiguration(Configuration));
			services.AddSingleton<IDocumentStore>(service =>
			{
				// Configure Marten
				var config = service.GetService<IConfiguration>();
				return DocumentStore.For(options =>
				{
					options.Connection(config.PostgresConnectionString);
					options.Schema.For<EncryptedItem>().Index(x => x.FriendlyId);
				});
			});
			services.AddScoped<SymmetricAlgorithm>(service => Aes.Create());
			services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();
			services.AddScoped<ISymmetricEncryptionProvider, SymmetricEncryptionProvider>();
			services.AddScoped<ITextRepository>(service =>
			{
				// Configure the default Repositroy
				var store = service.GetService<IDocumentStore>();
				return new TextRepository(store);
			});
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
					name: "Delete",
					template: "delete",
					defaults: new { controller = "Home", action = "Delete" }
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
