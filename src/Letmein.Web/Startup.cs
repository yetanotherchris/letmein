﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Letmein.Web
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = (IConfigurationRoot) configuration;
		}
		
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging();
			services.AddRouting();
			services.AddControllersWithViews();
			services.AddRazorPages();
			
			DependencyInjection.ConfigureServices(services, Configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
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