using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Letmein.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
								.Enrich.FromLogContext()
								.WriteTo.Console(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [{SourceContext:l}] {Message}{NewLine}{Exception}")
								.CreateLogger();

			try
			{
				// Configure web
				var host = new WebHostBuilder()
					.UseKestrel()
					.UseContentRoot(Directory.GetCurrentDirectory())
					.UseStartup<Startup>()
					.UseSerilog()
					.Build();

				// Start the cleanup service
				var cleanup = new Cleanup(host.Services);
				cleanup.StartBackgroundCleanup();

				// Start the web
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