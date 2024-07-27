using Letmein.Core.Repositories.FileSystem;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using CloudFileStore;
using Letmein.Core;
using Newtonsoft.Json;
using System.Linq;
using Shouldly;

namespace Letmein.Tests.Unit.Core.Repositories
{
	public class JsonTextFileRepositoryTests
	{
		[Fact]
		public async Task Save_should_jsonify_and_save_with_storage_provider()
		{
			// Arrange
			string expectedId = "my-id";
			var expectedItem = new EncryptedItem() { FriendlyId = expectedId };
			string expectedJson = JsonConvert.SerializeObject(expectedItem);

			var logger = Substitute.For<ILogger>();
			var storageProvider = Substitute.For<IStorageProvider>();

			var jsonRepository = new JsonTextFileRepository(logger, storageProvider);

			// Act
			await jsonRepository.Save(expectedItem);

			// Assert
			Received.InOrder(async () =>
			{
				await storageProvider.SaveTextFileAsync($"{expectedId}.json", expectedJson, "application/json")
									 .Received(1);
			});
		}

		[Fact]
		public async Task Load_should_use_storage_provider_with_filename()
		{
			// Arrange
			string expectedFilename = "friendly-id.json";

			var logger = Substitute.For<ILogger>();
			var storageProvider = Substitute.For<IStorageProvider>();
			storageProvider.FileExistsAsync(expectedFilename).Returns(Task.FromResult(true));

			var jsonRepository = new JsonTextFileRepository(logger, storageProvider);

			// Act
			await jsonRepository.Load("friendly-id");

			// Assert
			Received.InOrder(async () =>
			{
				await storageProvider.LoadTextFileAsync(expectedFilename).Received(1);
			});
		}

		[Fact]
		public async Task GetExpiredItems_should_use_expired_items_list()
		{
			// Arrange
			var item1 = new EncryptedItem() { FriendlyId = "friendly1", ExpiresOn = DateTime.Today.AddDays(-2) };
			var item2 = new EncryptedItem() { FriendlyId = "friendly2", ExpiresOn = DateTime.Today.AddDays(-2) };

			string item1Filename = $"{item1.FriendlyId}.json";
			string item2Filename = $"{item2.FriendlyId}.json";

			string item1Json = JsonConvert.SerializeObject(item1);
			string item2Json = JsonConvert.SerializeObject(item2);

			var fileList = new List<string>()
			{
				$"{item1.FriendlyId}.json",
				$"{item2.FriendlyId}.json"
			};

			var logger = Substitute.For<ILogger>();
			var storageProvider = Substitute.For<IStorageProvider>();

			storageProvider.ListFilesAsync(1000, false).Returns(Task.FromResult(fileList.AsEnumerable()));
			storageProvider.FileExistsAsync(item1Filename).Returns(Task.FromResult(true));
			storageProvider.FileExistsAsync(item2Filename).Returns(Task.FromResult(true));
			storageProvider.LoadTextFileAsync(item1Filename).Returns(Task.FromResult(item1Json));
			storageProvider.LoadTextFileAsync(item2Filename).Returns(Task.FromResult(item2Json));

			var jsonRepository = new JsonTextFileRepository(logger, storageProvider);
			await jsonRepository.FindAllItems();

			// Act
			var expiredItems = await jsonRepository.GetExpiredItems(DateTime.Now);

			// Assert
			expiredItems.Count().ShouldBe(2);
		}

		[Fact]
		public async Task Delete_should_use_storage_provider_with_filename()
		{
			// Arrange
			string expectedFilename = "friendly-id.json";

			var logger = Substitute.For<ILogger>();
			var storageProvider = Substitute.For<IStorageProvider>();
			storageProvider.FileExistsAsync(expectedFilename).Returns(Task.FromResult(true));

			var jsonRepository = new JsonTextFileRepository(logger, storageProvider);

			// Act
			await jsonRepository.Delete("friendly-id");

			// Assert
			Received.InOrder(async () =>
			{
				await storageProvider.DeleteFileAsync(expectedFilename).Received(1);
			});
		}
	}
}