using System;
using System.IO;
using Letmein.Core.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Letmein.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				// Configure web
				var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
					webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
					webBuilder.ConfigureAppConfiguration(config =>
					{
						config.SetBasePath(Directory.GetCurrentDirectory());
						config.AddJsonFile("appsettings.json", true);
						config.AddEnvironmentVariables();
					});
                }).Build();
				
				// Start the web
				var configuration = host.Services.GetService<ILetmeinConfiguration>();
				var logger = host.Services.GetService<ILogger<Program>>();
				logger.LogInformation("Using Repository type: '{repositoryType}'" , configuration.RepositoryType);

				host.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host or cleanup terminated unexpectedly");
				Environment.Exit(1);
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}