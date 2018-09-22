using System;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Configuration;
using Letmein.Core.Services;
using Letmein.Core.Services.UniqueId;
using Letmein.Tests.Unit.MocksAndStubs;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Shouldly;
using Xunit;

namespace Letmein.Tests.Unit.Core.Services
{
	public class TextEncryptionServiceTests
	{
		private Mock<IUniqueIdGenerator> _uniqueIdGeneratorMock;
		private TextRepositoryMock _repository;
		private TextEncryptionService _encryptionService;
		private ConfigurationStub _configuration;

		public TextEncryptionServiceTests()
		{
			ILoggerFactory loggingFactory = new LoggerFactory().AddSerilog();

			_configuration = new ConfigurationStub();
			_uniqueIdGeneratorMock = new Mock<IUniqueIdGenerator>();
			_repository = new TextRepositoryMock();
			_encryptionService = new TextEncryptionService(_uniqueIdGeneratorMock.Object, _repository, loggingFactory, _configuration);
		}

		[Fact]
		public async Task StoredEncryptedJson_should_persist_to_repository_and_set_expiry()
		{
			// Arrange
			string friendlyId = "my FriendlyId";
			string json = "{ encrypted json }";
			int expiresInMinutes = 60 * 12;

			// Act
			string newUrl = await _encryptionService.StoredEncryptedJson(json, friendlyId, expiresInMinutes);

			// Assert
			EncryptedItem actualSavedItem = _repository.SavedEncryptedItem;

			newUrl.ShouldBe(friendlyId);
			actualSavedItem.Id.ShouldNotBe(Guid.Empty);
			actualSavedItem.FriendlyId.ShouldBe(friendlyId);
			actualSavedItem.CipherJson.ShouldBe(json);
			actualSavedItem.CreatedOn.ShouldBeGreaterThanOrEqualTo(DateTime.Today);
			actualSavedItem.ExpiresOn.ShouldBe(actualSavedItem.CreatedOn.AddMinutes(expiresInMinutes));
		}

		[Fact]
		public async Task StoredEncryptedJson_generate_unique_id_when_id_is_empty()
		{
			// Arrange
			string json = "{ encrypted json }";
			string friendlyId = "";
			string expectedId = "short-id";
			_configuration.IdGenerationType = IdGenerationType.ShortCode;
			_uniqueIdGeneratorMock.Setup(x => x.Generate(IdGenerationType.ShortCode)).Returns(expectedId);

			// Act
			string newId = await _encryptionService.StoredEncryptedJson(json, friendlyId, 90);

			// Assert
			newId.ShouldBe(expectedId);
		}

		[Fact]
		public async Task LoadEncryptedJson_should_load_json_by_uniqueid()
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
			EncryptedItem actualEncryptedItem = await _encryptionService.LoadEncryptedJson(friendlyId);

			// Assert
			actualEncryptedItem.CipherJson.ShouldBe(json);
		}

		[Fact]
		public async Task LoadEncryptedJson_should_return_null_when_load_fails()
		{
			// Arrange
			string friendlyId = "nIce-Id";

			// Act
			EncryptedItem actualEncryptedItem = await _encryptionService.LoadEncryptedJson(friendlyId);

			// Assert
			actualEncryptedItem.ShouldBeNull();
		}

		[Fact]
		public async Task Delete_should_remove_item_using_repository_and_return_true()
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
			bool result = await _encryptionService.Delete(friendlyId);

			// Assert
			result.ShouldBeTrue();
		}

		[Fact]
		public async Task Delete_should_return_false_when_delete_fails()
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
			bool result = await _encryptionService.Delete("myid");

			// Assert
			result.ShouldBeFalse();
		}
	}
}