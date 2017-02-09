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

namespace Letmein.Web
{
	public class Cleanup
	{
		private readonly ILogger<Cleanup> _logger;
		private readonly string _connectionString;
		private TimeSpan _defaultWaitTime;

		public Cleanup()
		{
			// Expected env variables:
			// - POSTGRES_CONNECTIONSTRING
			// - POLLER_WAIT_TIME (in seconds, optional, defaults to 30)

			// Setup Sirilog
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.LiterateConsole(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [Cleanup Service] {Message}{NewLine}{Exception}")
				.CreateLogger();

			// Configure MS Logging
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();
			_logger = loggingFactory.CreateLogger<Cleanup>();

			// Read environmental variables
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
			var config = configurationBuilder.Build();
			_connectionString = config["POSTGRES_CONNECTIONSTRING"];

			double pollWaitTime = 30d;
			if (!string.IsNullOrEmpty(config["POLLER_WAIT_TIME"]))
			{
				double.TryParse(config["POLLER_WAIT_TIME"], out pollWaitTime);
			}
			_defaultWaitTime = TimeSpan.FromSeconds(pollWaitTime);
		}

		public void StartBackgroundCleanup()
		{
			// Begin
			_logger.LogInformation("Starting...");
			_logger.LogInformation("By default I will sleep {0} seconds between checks.", _defaultWaitTime);

			Task.Run(() =>
			{
				var repository = new TextRepository(_connectionString);

				while (true)
				{
					DateTime now = DateTime.UtcNow;
					IEnumerable<EncryptedItem> items = repository.GetExpiredItems(now);

					_logger.LogInformation("{0} expired items found", items.Count());

					foreach (EncryptedItem item in items)
					{
						repository.Delete(item);
						_logger.LogInformation("Deleted item '{0}' as its expiry date is '{1}'", item.FriendlyId, item.CreatedOn);
					}

					_logger.LogInformation("Sleeping for {0} seconds", _defaultWaitTime.TotalSeconds);
					Thread.Sleep(_defaultWaitTime);
				}
			});
		}
	}
}