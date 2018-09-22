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

		public async Task<EncryptedItem> Load(string friendlyId)
		{
			return EncryptedItems.FirstOrDefault(x => x.FriendlyId == friendlyId);
		}

		public async Task Save(EncryptedItem encryptedItem)
		{
			SavedEncryptedItem = encryptedItem;
		}

		public async Task<IEnumerable<EncryptedItem>> GetExpiredItems(DateTime beforeDate)
		{
			return null;
		}

		public async Task Delete(string friendlyId)
		{
			if (DeleteThrows)
				throw new Exception("Delete failed");
		}
	}
}