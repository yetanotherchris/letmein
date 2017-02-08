using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Letmein.Cleanup
{
    public class Program
    {
        public static void Main(string[] args)
        {
			// Expected env variables:
			// - POSTGRES_CONNECTIONSTRING
			// - POLLER_WAIT_TIME

			// Setup Sirilog
			Log.Logger = new LoggerConfiguration()
					  .Enrich.FromLogContext()
					  .WriteTo.LiterateConsole()
					  .CreateLogger();

			// Configure MS Logging
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
	        IConfigurationRoot config = configurationBuilder.Build();
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();
			var logger = loggingFactory.CreateLogger<Program>();

			// Read environmental variables
			string connectionString = config["POSTGRES_CONNECTIONSTRING"];
			connectionString = "host=localhost;database=letmein;password=letmein123;username=letmein";

			double pollWaitTime = 30d;
	        if (!string.IsNullOrEmpty(config["POLLER_WAIT_TIME"]))
	        {
		        double.TryParse(config["POLLER_WAIT_TIME"], out pollWaitTime);
	        }
	        TimeSpan defaultWaitTime = TimeSpan.FromSeconds(pollWaitTime);
	        logger.LogInformation("By default I will wait {0} between polls", defaultWaitTime);

			// Sit and poll
			var repository = new TextRepository(connectionString);
	        TimeSpan noopWaitTime = defaultWaitTime;

			while (true)
			{
				DateTime now = DateTime.UtcNow;
				IEnumerable<EncryptedItem> items = repository.GetExpiredItems(now);

				foreach (EncryptedItem item in items)
				{
					repository.Delete(item);
					logger.LogInformation("Deleted item '{0}' as its expiry date is '{1}'", item.FriendlyId, item.CreatedOn);
				}

				logger.LogInformation("Waiting {0} seconds", noopWaitTime.TotalSeconds);
				Thread.Sleep(noopWaitTime);
			}
		}
    }
}
