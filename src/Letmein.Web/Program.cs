using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Letmein.Core.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Letmein.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Configure web
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build();

			// Start the cleanup service
			var cleanup = new Cleanup(host.Services);
			cleanup.StartBackgroundCleanup();

			// Start the web
			host.Run();
		}
	}
}