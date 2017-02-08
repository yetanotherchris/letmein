using System;
using System.Collections.Generic;

namespace Letmein.Core.Repositories
{
    public interface ITextRepository
    {
        EncryptedItem Load(string url);
        void Save(EncryptedItem encryptedItem);
		IEnumerable<EncryptedItem> GetExpiredItems(DateTime beforeDate);
	    void Delete(EncryptedItem encryptedItem);
    }
}