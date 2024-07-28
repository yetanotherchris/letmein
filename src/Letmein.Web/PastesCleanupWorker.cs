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
    public class PastesCleanupWorker : BackgroundService
	{
		private readonly ILogger<PastesCleanupWorker> _logger;
        private readonly ITextRepository _textRepository;
        private readonly TimeSpan _defaultWaitTime;
	
		public PastesCleanupWorker(ILogger<PastesCleanupWorker> logger, ILetmeinConfiguration configuration, ITextRepository textRepository)
		{
			_logger = logger;
			_textRepository = textRepository;
			_defaultWaitTime = TimeSpan.FromSeconds(configuration.CleanupSleepTime);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
           	_logger.LogInformation("Starting CleanupWorker...");
			_logger.LogInformation("By default I will sleep {0} seconds between checks.", _defaultWaitTime);

			return Task.Run(async () =>
			{
				try
				{
					while (!stoppingToken.IsCancellationRequested)
					{
						DateTime now = DateTime.UtcNow;

						IEnumerable<EncryptedItem> items = await _textRepository.GetExpiredItems(now);
						_logger.LogInformation("{0} expired items found", items.Count());

						foreach (EncryptedItem item in items)
						{
							await _textRepository.Delete(item.FriendlyId);
							_logger.LogInformation("Deleted item {0} as its expiry date is {1}", item.FriendlyId, item.CreatedOn);
						}

						_logger.LogInformation("Sleeping for {0} seconds", _defaultWaitTime.TotalSeconds);
						
						await Task.Delay(_defaultWaitTime, stoppingToken);
					}
				}
				catch (Exception)
				{
				}
			}, stoppingToken);
		}
    }
}