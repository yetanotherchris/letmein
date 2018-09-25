﻿using System;
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
using IConfiguration = Letmein.Core.Configuration.ILetmeinConfiguration;
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
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
			services.AddLogging();

			DependencyInjection.ConfigureServices(services, Configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
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
	}
}