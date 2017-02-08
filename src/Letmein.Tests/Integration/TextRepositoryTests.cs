using System;
using System.Linq;
using Letmein.Core;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.Postgres;
using NUnit.Framework;

namespace Letmein.Tests.Integration
{
	[TestFixture]
	public class TextRepositoryTests
	{
		private string _connectionString = "host=localhost;database=letmein;password=letmein123;username=letmein";
		private TextRepository _repository;

		[SetUp]
		public void Setup()
		{
			_repository = new TextRepository(_connectionString);
			_repository.ClearDatabase();
		}

		[Test]
		public void Load_should_loads_entity_and_Save_should_store()
		{
			// Arrange
			var encryptedItem = new EncryptedItem()
			{
				Id = Guid.NewGuid(),
				AlgorithmName = "AES",
				CipherJson = new string('a', 512 * 1024 * 2),
				FriendlyId = "foo",
				CreatedOn = DateTime.UtcNow
			};

			_repository.Save(encryptedItem);

			// Act
			EncryptedItem loadedItem = _repository.Load(encryptedItem.FriendlyId);

			// Assert
			Assert.NotNull(loadedItem);
			Assert.That(loadedItem.Id, Is.EqualTo(encryptedItem.Id));
			Assert.That(loadedItem.AlgorithmName, Is.EqualTo(encryptedItem.AlgorithmName));
			Assert.That(loadedItem.FriendlyId, Is.EqualTo(encryptedItem.FriendlyId));
			Assert.That(loadedItem.CreatedOn, Is.EqualTo(encryptedItem.CreatedOn));
		}

		[Test]
		public void GetExpireItems_should_get_old_items()
		{
			// Arrange
			var encryptedItem1 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id1", ExpiresOn = DateTime.Today.AddMinutes(1) };
			var encryptedItem2 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id2", ExpiresOn = DateTime.Today.AddDays(-1) };
			var encryptedItem3 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id3", ExpiresOn = DateTime.Today.AddDays(-2) };

			_repository.Save(encryptedItem1);
			_repository.Save(encryptedItem2);
			_repository.Save(encryptedItem3);

			// Act
			var items = _repository.GetExpiredItems(DateTime.Today);

			// Assert
			Assert.That(items.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Delete_should_remove_expected_item()
		{
			// Arrange
			var encryptedItem1 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id1" };
			var encryptedItem2 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id2" };
			var encryptedItem3 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id3" };

			_repository.Save(encryptedItem1);
			_repository.Save(encryptedItem2);
			_repository.Save(encryptedItem3);

			// Act
			_repository.Delete(encryptedItem2);
			var items = _repository.All();

			// Assert
			Assert.That(items.Count(), Is.EqualTo(2));
			Assert.That(items.Last().FriendlyId, Is.EqualTo("id3"));
		}
	}
}