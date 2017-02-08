using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Letmein.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
	        var cleanup = new Cleanup();
	        cleanup.StartBackgroundCleanup();

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
