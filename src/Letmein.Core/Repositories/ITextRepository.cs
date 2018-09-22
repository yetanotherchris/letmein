using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Letmein.Core.Repositories
{
	public interface ITextRepository
	{
		Task<EncryptedItem> Load(string friendlyId);

		Task Save(EncryptedItem encryptedItem);

		Task<IEnumerable<EncryptedItem>> GetExpiredItems(DateTime beforeDate);

		Task Delete(string friendlyId);
	}
}