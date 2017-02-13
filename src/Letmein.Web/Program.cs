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
			// Cleanup service
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
			var configurationRoot = configurationBuilder.Build();
			var configuration = new DefaultConfiguration(configurationRoot);

			var cleanup = new Cleanup(configuration);
	        cleanup.StartBackgroundCleanup();

			// Website
			var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
