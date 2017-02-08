using System;
using Letmein.Core;
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

		[SetUp]
		public void Setup()
		{
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();

			_uniqueIdGeneratorMock = new Mock<IUniqueIdGenerator>();
			_repository = new TextRepositoryMock();
			_encryptionService = new TextEncryptionService(_uniqueIdGeneratorMock.Object, _repository, loggingFactory);
		}

		[Test]
		public void StoredEncryptedJson_should_persist_to_repository_and_set_expiry_to_12_hours()
		{
			// Arrange
			string friendlyId = "my FriendlyId";
			string json = "{ encrypted json }";

			// Act
			string newUrl = _encryptionService.StoredEncryptedJson(json, friendlyId);

			// Assert
			EncryptedItem actualSavedItem = _repository.SavedEncryptedItem;

			Assert.That(newUrl, Is.EqualTo(friendlyId));
			Assert.That(actualSavedItem.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(actualSavedItem.FriendlyId, Is.EqualTo(friendlyId));
			Assert.That(actualSavedItem.CipherJson, Is.EqualTo(json));
			Assert.That(actualSavedItem.CreatedOn, Is.GreaterThanOrEqualTo(DateTime.Today));
			Assert.That(actualSavedItem.ExpiresOn, Is.EqualTo(actualSavedItem.CreatedOn.AddHours(12)));
		}

		[Test]
		public void StoredEncryptedJson_generate_unique_id_when_id_is_empty()
		{
			// Arrange
			string json = "{ encrypted json }";
			string friendlyId = "";

			_uniqueIdGeneratorMock.Setup(x => x.Generate()).Returns("my id");

			// Act
			string newId = _encryptionService.StoredEncryptedJson(json, friendlyId);

			// Assert
			Assert.That(newId, Is.EqualTo("my id"));
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
	}
}