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
    public class NotesCleanupJob : IJob
    {
        private readonly ILogger<NotesCleanupJob> _logger;
        private readonly ITextRepository _textRepository;

        public NotesCleanupJob(ILogger<NotesCleanupJob> logger, ITextRepository textRepository)
        {
            _logger = logger;
            _textRepository = textRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting notes cleanup job...");

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

                _logger.LogInformation("Notes cleanup job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notes cleanup job");
                throw;
            }
        }
    }
}
