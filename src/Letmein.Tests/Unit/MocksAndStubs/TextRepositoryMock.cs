using System;
using System.Collections.Generic;
using System.Linq;
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

		public EncryptedItem Load(string friendlyId)
		{
			return EncryptedItems.FirstOrDefault(x => x.FriendlyId == friendlyId);
		}

		public void Save(EncryptedItem encryptedItem)
		{
			SavedEncryptedItem = encryptedItem;
		}

		public IEnumerable<EncryptedItem> GetExpiredItems(DateTime beforeDate)
		{
			yield break;
		}

		public void Delete(string friendlyId)
		{
			if (DeleteThrows)
				throw new Exception("Delete failed");
		}
	}
}