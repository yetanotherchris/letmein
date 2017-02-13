using System;
using Letmein.Core.Configuration;
using Letmein.Core.Encryption;
using Letmein.Core.Repositories;
using Letmein.Core.Services.UniqueId;
using Microsoft.Extensions.Logging;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Letmein.Core.Services
{
	public class TextEncryptionService : ITextEncryptionService
	{
		private readonly IUniqueIdGenerator _idGenerator;
		private readonly ITextRepository _repository;
		private readonly IConfiguration _configuration;
		private readonly ILogger<TextEncryptionService> _logger;

		public TextEncryptionService(IUniqueIdGenerator idGenerator, ITextRepository repository, ILoggerFactory loggingFactory, IConfiguration configuration)
		{
			_idGenerator = idGenerator;
			_repository = repository;
			_configuration = configuration;
			_logger = loggingFactory.CreateLogger<TextEncryptionService>();
		}

		public string StoredEncryptedJson(string json, string friendlyId)
		{
			try
			{
				if (string.IsNullOrEmpty(friendlyId))
				{
					friendlyId = _idGenerator.Generate();
				}

				var createdate = DateTime.UtcNow;
				var encryptedItem = new EncryptedItem()
				{
					Id = Guid.NewGuid(),
					FriendlyId = friendlyId,
					AlgorithmName = "STANFORDV1",
					CreatedOn = createdate,
					ExpiresOn = createdate.AddSeconds(_configuration.ExpirePastesAfter),
					CipherJson = json,
				};

				_repository.Save(encryptedItem);

				return friendlyId;
			}
			catch (EncryptionException ex)
			{
				_logger.LogInformation($"StoredEncryptedJson failed for '{friendlyId}': " + ex);
			}

			return "";
		}

		public EncryptedItem LoadEncryptedJson(string friendlyId)
		{
			try
			{
				return _repository.Load(friendlyId);
			}
			catch (EncryptionException ex)
			{
				_logger.LogInformation($"StoredEncryptedJson failed for '{friendlyId}': " + ex);
			}

			return null;
		}
	}
}