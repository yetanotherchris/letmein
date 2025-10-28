using System;
using System.Linq;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Letmein.Api.Jobs
{
    [DisallowConcurrentExecution]
    public class PastesCleanupJob : IJob
    {
        private readonly ILogger<PastesCleanupJob> _logger;
        private readonly ITextRepository _textRepository;

        public PastesCleanupJob(ILogger<PastesCleanupJob> logger, ITextRepository textRepository)
        {
            _logger = logger;
            _textRepository = textRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting pastes cleanup job...");

            try
            {
                DateTime now = DateTime.UtcNow;

                var items = await _textRepository.GetExpiredItems(now);
                _logger.LogInformation("{Count} expired items found", items.Count());

                foreach (EncryptedItem item in items)
                {
                    await _textRepository.Delete(item.FriendlyId);
                    _logger.LogInformation("Deleted item {FriendlyId} with expiry date {ExpiryDate}",
                        item.FriendlyId, item.CreatedOn);
                }

                _logger.LogInformation("Pastes cleanup job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during pastes cleanup job");
                throw;
            }
        }
    }
}
