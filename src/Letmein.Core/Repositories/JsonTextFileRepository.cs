using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudFileStore;
using Letmein.Core.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Letmein.Core.Repositories.FileSystem
{
	public class JsonTextFileRepository : ITextRepository
	{
		private readonly ILogger _logger;
		private readonly IStorageProvider _storageProvider;
		private List<ExpiryItem> _itemExpirys;

		public JsonTextFileRepository(ILogger logger, IStorageProvider storageProvider)
		{
			_logger = logger;
			_storageProvider = storageProvider;

			FindAllExpiryItems().GetAwaiter().GetResult();
		}

		public async Task FindAllExpiryItems()
		{
			_itemExpirys = new List<ExpiryItem>();

			foreach (string filename in await _storageProvider.ListFilesAsync(1000, false))
			{
				EncryptedItem item = await Load(filename.Replace(".json", ""));

				if (item != null)
				{
					var expiryItem = new ExpiryItem()
					{
						EncryptedItemId = item.FriendlyId,
						ExpiryDate = item.ExpiresOn
					};

					_itemExpirys.Add(expiryItem);
				}
			}
		}

		public async Task<EncryptedItem> Load(string friendlyId)
		{
			string filename = $"{friendlyId}.json";

			if (!await _storageProvider.FileExistsAsync(filename))
				return null;

			try
			{
				string json = await _storageProvider.LoadTextFileAsync(filename);
				var item = JsonConvert.DeserializeObject<EncryptedItem>(json);

				return item;
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Unable to de-serialize {filename} - {ex}");
				return null;
			}
		}

		public async Task Save(EncryptedItem encryptedItem)
		{
			string filename = $"{encryptedItem.FriendlyId}.json";

			try
			{
				string json = JsonConvert.SerializeObject(encryptedItem);
				await _storageProvider.SaveTextFileAsync(filename, json, "application/json");

				var expiryItem = new ExpiryItem()
				{
					EncryptedItemId = encryptedItem.FriendlyId,
					ExpiryDate = encryptedItem.ExpiresOn
				};
				_itemExpirys.Add(expiryItem);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Unable to serialize {filename} - {ex}");
			}
		}

		public async Task<IEnumerable<EncryptedItem>> GetExpiredItems(DateTime beforeDate)
		{
			IEnumerable<ExpiryItem> expiredIds = _itemExpirys.Where(x => x.ExpiryDate <= beforeDate);

			var expiredItems = new List<EncryptedItem>();
			foreach (ExpiryItem expiryItem in expiredIds)
			{
				EncryptedItem encryptedItem = await Load(expiryItem.EncryptedItemId);

				if (encryptedItem != null)
					expiredItems.Add(encryptedItem);
			}

			return expiredItems;
		}

		public async Task Delete(string friendlyId)
		{
			string filename = $"{friendlyId}.json";

			if (!await _storageProvider.FileExistsAsync(filename))
				return;

			try
			{
				await _storageProvider.DeleteFileAsync(filename);

				var expiryItem = _itemExpirys.FirstOrDefault(x => x.EncryptedItemId == friendlyId);
				_itemExpirys.Remove(expiryItem);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Unable to delete {filename} - {ex}");
			}
		}

		private class ExpiryItem
		{
			public DateTime ExpiryDate { get; set; }
			public string EncryptedItemId { get; set; }

			public override bool Equals(object obj)
			{
				if (obj is ExpiryItem other)
				{
					return other.EncryptedItemId.Equals(EncryptedItemId);
				}

				return false;
			}
		}
	}
}