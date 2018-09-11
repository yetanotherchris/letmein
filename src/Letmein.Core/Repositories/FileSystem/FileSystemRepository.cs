using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Letmein.Core.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Letmein.Core.Repositories.FileSystem
{
	public class FileSystemRepository : ITextRepository
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;
		private List<ExpiryItem> _itemExpirys;

		private class ExpiryItem
		{
			public DateTime ExpiryDate { get; set; }
			public string EncryptedItemId { get; set; }
		}

		public FileSystemRepository(IConfiguration configuration, ILogger logger)
		{
			_configuration = configuration;

			if (!Directory.Exists(_configuration.NotesPath))
				throw new ConfigurationException($"The notes path '{_configuration.NotesPath}' does not exist.");

			_logger = logger;
			_itemExpirys = new List<ExpiryItem>();
			TrackAllExpiryDates();
		}

		private void TrackAllExpiryDates()
		{
			foreach (string file in Directory.EnumerateFiles(_configuration.NotesPath, "*.json"))
			{
				var fileInfo = new FileInfo(file);
				EncryptedItem item = Load(fileInfo.Name);

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
			string fullPath = Path.Combine(_configuration.NotesPath, $"{friendlyId}.json");

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
			string fullPath = Path.Combine(_configuration.NotesPath, $"{encryptedItem.FriendlyId}.json");

			try
			{
				string json = JsonConvert.SerializeObject(encryptedItem);
				File.WriteAllText(fullPath, json);
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Unable to serialize {fullPath} - {ex}");
				throw;
			}
		}

		public IEnumerable<EncryptedItem> GetExpiredItems(DateTime beforeDate)
		{
			throw new NotImplementedException();
		}

		public void Delete(string friendlyId)
		{
			throw new NotImplementedException();
		}
	}
}