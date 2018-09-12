using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Letmein.Core.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Letmein.Core.Repositories.FileSystem
{
	public class FileSystemRepository : ITextRepository
	{
		private readonly ILogger _logger;
		private List<ExpiryItem> _itemExpirys;

		public string _pastesFullPath { get; }

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

		public FileSystemRepository(IConfiguration configuration, ILogger logger)
		{
			var directory = new DirectoryInfo(configuration.PastesStorePath);
			_pastesFullPath = directory.FullName;

			if (!Directory.Exists(_pastesFullPath))
				throw new ConfigurationException($"The pastes path '{_pastesFullPath}' does not exist.");

			_logger = logger;
			ScanPastesDirectoryForExpiredItems();
		}

		public void ScanPastesDirectoryForExpiredItems()
		{
			_itemExpirys = new List<ExpiryItem>();

			foreach (string file in Directory.EnumerateFiles(_pastesFullPath, "*.json"))
			{
				var fileInfo = new FileInfo(file);
				EncryptedItem item = Load(fileInfo.FullName.Replace(".json", ""));

				var expiryItem = new ExpiryItem()
				{
					EncryptedItemId = item.FriendlyId,
					ExpiryDate = item.ExpiresOn
				};
				_itemExpirys.Add(expiryItem);
			}
		}

		public EncryptedItem Load(string friendlyId)
		{
			string fullPath = Path.Combine(_pastesFullPath, $"{friendlyId}.json");

			if (!File.Exists(fullPath))
				return null;

			try
			{
				string json = File.ReadAllText(fullPath);
				var item = JsonConvert.DeserializeObject<EncryptedItem>(json);

				return item;
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Unable to de-serialize {fullPath} - {ex}");
				return null;
			}
		}

		public void Save(EncryptedItem encryptedItem)
		{
			string fullPath = Path.Combine(_pastesFullPath, $"{encryptedItem.FriendlyId}.json");

			try
			{
				string json = JsonConvert.SerializeObject(encryptedItem);
				File.WriteAllText(fullPath, json);

				var expiryItem = new ExpiryItem()
				{
					EncryptedItemId = encryptedItem.FriendlyId,
					ExpiryDate = encryptedItem.ExpiresOn
				};
				_itemExpirys.Add(expiryItem);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Unable to serialize {fullPath} - {ex}");
			}
		}

		public IEnumerable<EncryptedItem> GetExpiredItems(DateTime beforeDate)
		{
			IEnumerable<ExpiryItem> expiredIds = _itemExpirys.Where(x => x.ExpiryDate <= beforeDate);

			var expiredItems = new List<EncryptedItem>();
			foreach (ExpiryItem expiryItem in expiredIds)
			{
				EncryptedItem encryptedItem = Load(expiryItem.EncryptedItemId);

				if (encryptedItem != null)
					expiredItems.Add(encryptedItem);
			}

			return expiredItems;
		}

		public void Delete(string friendlyId)
		{
			string fullPath = Path.Combine(_pastesFullPath, $"{friendlyId}.json");

			if (!File.Exists(fullPath))
				return;

			try
			{
				File.Delete(fullPath);

				var expiryItem = _itemExpirys.FirstOrDefault(x => x.EncryptedItemId == friendlyId);
				_itemExpirys.Remove(expiryItem);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Unable to delete {fullPath} - {ex}");
			}
		}
	}
}