using System;
using Letmein.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
                // Configure
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
				var host = LetmeInStartupBuilder.Configure(builder);
				
				// Start
				var configuration = host.Services.GetService<ILetmeinConfiguration>();
				var logger = host.Services.GetService<ILogger<Program>>();
				logger.LogInformation("Using Repository type: '{repositoryType}'" , configuration.RepositoryType);

				host.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Host or cleanup terminated unexpectedly:\n" + ex.ToString());
				Environment.Exit(1);
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}