using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Letmein.Core;
using Letmein.Core.Repositories;

namespace Letmein.Tests.Unit.MocksAndStubs
{
	public class TextRepositoryMock : ITextRepository
	{
		public List<EncryptedItem> EncryptedItems { get; set; }
		public EncryptedItem SavedEncryptedItem { get; set; }
		public bool DeleteThrows { get; set; }

		public TextRepositoryMock()
		{
			EncryptedItems = new List<EncryptedItem>();
		}

		public Task<EncryptedItem> Load(string friendlyId)
		{
			return Task.FromResult(EncryptedItems.FirstOrDefault(x => x.FriendlyId == friendlyId));
		}

		public Task Save(EncryptedItem encryptedItem)
		{
			SavedEncryptedItem = encryptedItem;
			return Task.CompletedTask;
		}

		public Task<IEnumerable<EncryptedItem>> GetExpiredItems(DateTime beforeDate)
		{
			return Task.FromResult(Enumerable.Empty<EncryptedItem>());
		}

		public Task Delete(string friendlyId)
		{
			if (DeleteThrows)
				throw new Exception("Delete failed");

			return Task.CompletedTask;
		}
	}
}