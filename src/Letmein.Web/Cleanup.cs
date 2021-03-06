using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.Postgres;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILetmeinConfiguration = Letmein.Core.Configuration.ILetmeinConfiguration;

namespace Letmein.Web
{
	public class Cleanup
	{
		private readonly ILogger<Cleanup> _logger;
		private readonly IServiceProvider _serviceProvider;
		private TimeSpan _defaultWaitTime;

		public Cleanup(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			// Logging
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();
			_logger = loggingFactory.CreateLogger<Cleanup>();

			// Config
			var configuration = _serviceProvider.GetService<ILetmeinConfiguration>();
			_defaultWaitTime = TimeSpan.FromSeconds(configuration.CleanupSleepTime);
		}

		public void StartBackgroundCleanup()
		{
			// Begin
			_logger.LogInformation("Starting...");
			_logger.LogInformation("By default I will sleep {0} seconds between checks.", _defaultWaitTime);

			Task.Run(async () =>
			{
				while (true)
				{
					DateTime now = DateTime.UtcNow;

					// A new one each poll, to work around the FileSystemRepository
					// needing to scan each time...could be improved.
					var textRepository = _serviceProvider.GetRequiredService<ITextRepository>();

					IEnumerable<EncryptedItem> items = await textRepository.GetExpiredItems(now);

					_logger.LogInformation("{0} expired items found", items.Count());

					foreach (EncryptedItem item in items)
					{
						await textRepository.Delete(item.FriendlyId);
						_logger.LogInformation("Deleted item {0} as its expiry date is {1}", item.FriendlyId, item.CreatedOn);
					}

					_logger.LogInformation("Sleeping for {0} seconds", _defaultWaitTime.TotalSeconds);
					Thread.Sleep(_defaultWaitTime);
				}
			});
		}
	}
}