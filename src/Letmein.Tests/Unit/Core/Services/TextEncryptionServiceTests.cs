using System;
using Letmein.Core;
using Letmein.Core.Configuration;
using Letmein.Core.Encryption;
using Letmein.Core.Services;
using Letmein.Core.Services.UniqueId;
using Letmein.Tests.Unit.MocksAndStubs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Serilog;

namespace Letmein.Tests.Unit.Core.Services
{
	public class TextEncryptionServiceTests
	{
		private Mock<IUniqueIdGenerator> _uniqueIdGeneratorMock;
		private TextRepositoryMock _repository;
		private TextEncryptionService _encryptionService;
		private ConfigurationStub _configuration;

		[SetUp]
		public void Setup()
		{
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();

			_configuration = new ConfigurationStub();
			_uniqueIdGeneratorMock = new Mock<IUniqueIdGenerator>();
			_repository = new TextRepositoryMock();
			_encryptionService = new TextEncryptionService(_uniqueIdGeneratorMock.Object, _repository, loggingFactory);
		}

		[Test]
		public void StoredEncryptedJson_should_persist_to_repository_and_set_expiry()
		{
			// Arrange
			string friendlyId = "my FriendlyId";
			string json = "{ encrypted json }";
			int expiresInMinutes = 60 * 12;

			string json = "{ encrypted json }";

			// Act
			string newUrl = _encryptionService.StoredEncryptedJson(json, friendlyId, expiresInMinutes);

			// Assert
			EncryptedItem actualSavedItem = _repository.SavedEncryptedItem;

			Assert.That(newUrl, Is.EqualTo(friendlyId));
			Assert.That(actualSavedItem.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(actualSavedItem.FriendlyId, Is.EqualTo(friendlyId));
			Assert.That(actualSavedItem.CipherJson, Is.EqualTo(json));
			Assert.That(actualSavedItem.CreatedOn, Is.GreaterThanOrEqualTo(DateTime.Today));
			Assert.That(actualSavedItem.ExpiresOn, Is.EqualTo(actualSavedItem.CreatedOn.AddMinutes(expiresInMinutes)));
		}

		[Test]
		public void StoredEncryptedJson_generate_unique_id_when_id_is_empty()
		{
			// Arrange
			string json = "{ encrypted json }";
			string friendlyId = "";
			int expiresInMinutes = 60 * 12;

			// Act
			string newId = _encryptionService.StoredEncryptedJson(json, friendlyId, expiresInMinutes);

			// Assert
			Assert.That(newId, Is.EqualTo(expectedId));
		}

		[Test]
		public void LoadEncryptedJson_should_load_json_by_uniqueid()
		{
			// Arrange
			string friendlyId = "nIce-Id";
			string json = "{json}";

			var expectedEncryptedItem = new EncryptedItem
			{
				FriendlyId = friendlyId,
				CipherJson = json
			};

			_repository.EncryptedItems.Add(expectedEncryptedItem);

			// Act
			EncryptedItem actualEncryptedItem = _encryptionService.LoadEncryptedJson(friendlyId);

			// Assert
			Assert.That(actualEncryptedItem.CipherJson, Is.EqualTo(json));
		}

		[Test]
		public void LoadEncryptedJson_should_return_null_when_load_fails()
		{
			// Arrange
			string friendlyId = "nIce-Id";

			// Act
			EncryptedItem actualEncryptedItem = _encryptionService.LoadEncryptedJson(friendlyId);

			// Assert
			Assert.That(actualEncryptedItem, Is.Null);
		}

		[Test]
		public void Delete_should_remove_item_using_repository_and_return_true()
		{
			// Arrange
			string friendlyId = "myid";
			var expectedEncryptedItem = new EncryptedItem
			{
				FriendlyId = friendlyId,
				CipherJson = ""
			};

			_repository.EncryptedItems.Add(expectedEncryptedItem);
			
			// Act
			bool result = _encryptionService.Delete(friendlyId);

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		public void Delete_should_return_false_when_delete_fails()
		{
			// Arrange
			_repository.DeleteThrows = true;

			string friendlyId = "myid";
			var expectedEncryptedItem = new EncryptedItem
			{
				FriendlyId = friendlyId,
				CipherJson = ""
			};

			_repository.EncryptedItems.Add(expectedEncryptedItem);

			// Act
			bool result = _encryptionService.Delete("myid");

			// Assert
			Assert.That(result, Is.False);
		}
	}
}