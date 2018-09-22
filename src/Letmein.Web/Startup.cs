using System;
using System.IO;
using System.Security.Cryptography;
using CloudFileStore.AWS;
using CloudFileStore.Azure;
using CloudFileStore.GoogleCloud;
using Letmein.Core;
using Letmein.Core.Configuration;
using Letmein.Core.Encryption;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.FileSystem;
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
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Letmein.Web
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }

		public Startup()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.development.json", optional: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
			services.AddLogging();

			services.AddSingleton<IConfigurationRoot>(sp => Configuration);
			var configuration = new Configuration(Configuration);

			services.AddSingleton((System.Func<System.IServiceProvider, IConfiguration>)(sp => configuration));
			ConfigureRepository(services, configuration, Configuration);

			services.AddScoped<SymmetricAlgorithm>(service => Aes.Create());
			services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();
			services.AddScoped<ISymmetricEncryptionProvider, SymmetricEncryptionProvider>();

			services.AddScoped<ITextEncryptionService, TextEncryptionService>();
		}

		private static void ConfigureRepository(IServiceCollection services, IConfiguration configuration, IConfigurationRoot configurationRoot)
		{
			switch (configuration.RepositoryType)
			{
				case RepositoryType.FileSystem:
					break;

				case RepositoryType.S3:
					ConfigureForS3(services, configurationRoot);
					break;

				case RepositoryType.GoogleCloud:
					ConfigureForGCloud(services, configurationRoot);
					break;

				case RepositoryType.AzureBlobs:
					ConfigureForAzureBlobs(services, configurationRoot);
					break;

				case RepositoryType.Postgres:
					ConfigureForPostgres(services);
					break;

				default:
					throw new NotSupportedException("Please enter a valid repository type");
			}

			if (configuration.RepositoryType == RepositoryType.Postgres)
			{
			}
			else
			{
			}
		}

		private static void ConfigureForS3(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				var s3Configuration = new S3Configuration();
				configurationRoot.Bind("S3", s3Configuration);

				var s3Provider = new S3StorageProvider(s3Configuration);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, s3Provider);
			});
		}

		private static void ConfigureForGCloud(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				var googleCloudConfig = new GoogleCloudConfiguration();
				configurationRoot.Bind("GoogleCloud", googleCloudConfig);

				var googleCloudProvider = new GoogleCloudStorageProvider(googleCloudConfig);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, googleCloudProvider);
			});
		}

		private static void ConfigureForAzureBlobs(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				var azureConfig = new AzureBlobConfiguration();
				configurationRoot.Bind("Azure", azureConfig);

				var azureProvider = new AzureBlobStorageProvider(azureConfig);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, azureProvider);
			});
		}

		private static void ConfigureForPostgres(IServiceCollection services)
		{
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

			services.AddScoped<ITextRepository>(service =>
			{
				var store = service.GetService<IDocumentStore>();
				return new PostgresTextRepository(store);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			// Setup Sirilog
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [Website] {Message}{NewLine}{Exception}")
				.CreateLogger();

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