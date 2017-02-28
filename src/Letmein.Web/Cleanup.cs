using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories.Postgres;
using Marten;
using Microsoft.Extensions.Logging;
using Serilog;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;

namespace Letmein.Web
{
	public class Cleanup
	{
		private readonly ILogger<Cleanup> _logger;
		private readonly string _connectionString;
		private TimeSpan _defaultWaitTime;

		public Cleanup(IConfiguration configuration)
		{
			// Sirilog for nicer looking console logs
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.LiterateConsole(Serilog.Events.LogEventLevel.Information, "[{Timestamp}] [Cleanup Service] {Message}{NewLine}{Exception}")
				.CreateLogger();

			// Configure MS Logging
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();
			_logger = loggingFactory.CreateLogger<Cleanup>();

			// Config
			_connectionString = configuration.PostgresConnectionString;
			_defaultWaitTime = TimeSpan.FromSeconds(configuration.CleanupSleepTime);
		}

		public void StartBackgroundCleanup()
		{
			// Begin
			_logger.LogInformation("Starting...");
			_logger.LogInformation("By default I will sleep {0} seconds between checks.", _defaultWaitTime);

			var store = DocumentStore.For(options =>
			{
				options.Connection(_connectionString);
				options.Schema.For<EncryptedItem>().Index(x => x.FriendlyId);
			});

			Task.Run(() =>
			{
				var repository = new TextRepository(store);

				while (true)
				{
					DateTime now = DateTime.UtcNow;
					IEnumerable<EncryptedItem> items = repository.GetExpiredItems(now);

					_logger.LogInformation("{0} expired items found", items.Count());

					foreach (EncryptedItem item in items)
					{
						repository.Delete(item.FriendlyId);
						_logger.LogInformation("Deleted item '{0}' as its expiry date is '{1}'", item.FriendlyId, item.CreatedOn);
					}

					_logger.LogInformation("Sleeping for {0} seconds", _defaultWaitTime.TotalSeconds);
					Thread.Sleep(_defaultWaitTime);
				}
			});
		}
	}
}