using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILetmeinConfiguration = Letmein.Core.Configuration.ILetmeinConfiguration;

namespace Letmein.Web
{
    public class CleanupWorker : IHostedService
	{
		private readonly ILogger<CleanupWorker> _logger;
        private readonly ITextRepository _textRepository;
        private readonly TimeSpan _defaultWaitTime;

		public CleanupWorker(ILogger<CleanupWorker> logger, ILetmeinConfiguration configuration, ITextRepository textRepository)
		{
			_logger = logger;
			_textRepository = textRepository;
			_defaultWaitTime = TimeSpan.FromSeconds(configuration.CleanupSleepTime);
		}

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return StartBackgroundCleanup(cancellationToken);
        }

		public Task StopAsync(CancellationToken cancellationToken)
        {
			return Task.CompletedTask;
        }

        public Task StartBackgroundCleanup(CancellationToken cancellationToken)
		{
			// Begin
			_logger.LogInformation("Starting...");
			_logger.LogInformation("By default I will sleep {0} seconds between checks.", _defaultWaitTime);

			return Task.Run(async () =>
			{
				while (true)
				{
					if (cancellationToken.IsCancellationRequested)
						break;

					DateTime now = DateTime.UtcNow;

					IEnumerable<EncryptedItem> items = await _textRepository.GetExpiredItems(now);
					_logger.LogInformation("{0} expired items found", items.Count());

					foreach (EncryptedItem item in items)
					{
						await _textRepository.Delete(item.FriendlyId);
						_logger.LogInformation("Deleted item {0} as its expiry date is {1}", item.FriendlyId, item.CreatedOn);
					}

					_logger.LogInformation("Sleeping for {0} seconds", _defaultWaitTime.TotalSeconds);
					
					await Task.Delay(_defaultWaitTime, cancellationToken);
				}
			}, cancellationToken);
		}
    }
}