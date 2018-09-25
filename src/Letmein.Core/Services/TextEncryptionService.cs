﻿using System;
using System.Threading.Tasks;
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
		private readonly ILetmeinConfiguration _configuration;
		private readonly ILogger<TextEncryptionService> _logger;

		public TextEncryptionService(IUniqueIdGenerator idGenerator, ITextRepository repository, ILoggerFactory loggingFactory, ILetmeinConfiguration configuration)
		{
			_idGenerator = idGenerator;
			_repository = repository;
			_configuration = configuration;
			_logger = loggingFactory.CreateLogger<TextEncryptionService>();
		}

		public async Task<string> StoredEncryptedJson(string json, string friendlyId, int expiresInMinutes)
		{
			try
			{
				if (string.IsNullOrEmpty(friendlyId))
				{
					friendlyId = _idGenerator.Generate(_configuration.IdGenerationType);
				}

				var createdate = DateTime.UtcNow;
				var encryptedItem = new EncryptedItem()
				{
					Id = Guid.NewGuid(),
					FriendlyId = friendlyId,
					AlgorithmName = "STANFORDV1",
					CreatedOn = createdate,
					ExpiresOn = createdate.AddMinutes(expiresInMinutes),
					CipherJson = json,
				};

				await _repository.Save(encryptedItem);

				return friendlyId;
			}
			catch (EncryptionException ex)
			{
				_logger.LogInformation($"StoredEncryptedJson failed for '{friendlyId}': " + ex);
			}

			return "";
		}

		public async Task<EncryptedItem> LoadEncryptedJson(string friendlyId)
		{
			return await _repository.Load(friendlyId);
		}

		public async Task<bool> Delete(string friendlyId)
		{
			try
			{
				await _repository.Delete(friendlyId);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError("Failed to delete {0} : {1}", friendlyId, ex.ToString());
				return false;
			}
		}
	}
}