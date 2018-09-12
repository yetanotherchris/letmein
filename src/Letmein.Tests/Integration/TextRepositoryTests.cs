﻿using System;
using System.Linq;
using Letmein.Core;
using Letmein.Core.Repositories.Postgres;
using Marten;
using Shouldly;
using Xunit;

namespace Letmein.Tests.Integration
{
	public class TextRepositoryTests
	{
		private PostgresTextRepository _repository;

		public TextRepositoryTests()
		{
			string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTIONSTRING");
			if (string.IsNullOrEmpty(connectionString))
				connectionString = "host=localhost;database=letmein;password=letmein123;username=letmein";

			var store = DocumentStore.For(options =>
			{
				options.Connection(connectionString);
				options.Schema.For<EncryptedItem>().Index(x => x.FriendlyId);
			});

			_repository = new PostgresTextRepository(store);
			_repository.ClearDatabase();
		}

		[Fact]
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
			loadedItem.ShouldNotBeNull();
			loadedItem.Id.ShouldBe(encryptedItem.Id);
			loadedItem.AlgorithmName.ShouldBe(encryptedItem.AlgorithmName);
			loadedItem.FriendlyId.ShouldBe(encryptedItem.FriendlyId);
			loadedItem.CreatedOn.ShouldBe(encryptedItem.CreatedOn);
		}

		[Fact]
		public void GetExpireItems_should_get_old_items_before_given_date()
		{
			// Arrange
			var encryptedItem1 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id1", ExpiresOn = DateTime.UtcNow.AddMinutes(1) };
			var encryptedItem2 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id2", ExpiresOn = DateTime.UtcNow.AddDays(-1) };
			var encryptedItem3 = new EncryptedItem() { Id = Guid.NewGuid(), FriendlyId = "id3", ExpiresOn = DateTime.UtcNow.AddDays(-2) };

			_repository.Save(encryptedItem1);
			_repository.Save(encryptedItem2);
			_repository.Save(encryptedItem3);

			// Act
			var items = _repository.GetExpiredItems(DateTime.UtcNow);

			// Assert
			items.Count().ShouldBe(2);
		}

		[Fact]
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
			_repository.Delete(encryptedItem2.FriendlyId);
			var items = _repository.All();

			// Assert
			items.Count().ShouldBe(2);
			items.Last().FriendlyId.ShouldBe("id3");
		}
	}
}