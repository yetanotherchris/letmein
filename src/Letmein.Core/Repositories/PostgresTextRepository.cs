using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Marten;

[assembly: InternalsVisibleTo("Letmein.Tests")]

namespace Letmein.Core.Repositories.Postgres
{
	public class PostgresTextRepository : ITextRepository
	{
		// docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=letmein -e POSTGRES_PASSWORD=letmein123 postgres
		// choco install dotnetcore-runtime

		private readonly IDocumentStore _store;

		public PostgresTextRepository(IDocumentStore store)
		{
			_store = store;
		}

		internal void ClearDatabase()
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.DeleteWhere<EncryptedItem>(x => true);
				session.SaveChanges();
			}
		}

		internal IEnumerable<EncryptedItem> All()
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return session.Query<EncryptedItem>().ToList();
			}
		}

		public async Task Save(EncryptedItem encryptedItem)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Store(encryptedItem);
				await session.SaveChangesAsync();
			}
		}

		public async Task<EncryptedItem> Load(string friendlyId)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session.Query<EncryptedItem>()
									.FirstOrDefaultAsync(x => x.FriendlyId == friendlyId);
			}
		}

		public async Task<IEnumerable<EncryptedItem>> GetExpiredItems(DateTime beforeDate)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return await session.Query<EncryptedItem>()
									.Where(x => x.ExpiresOn <= beforeDate)
									.ToListAsync<EncryptedItem>();
			}
		}

		public async Task Delete(string friendlyId)
		{
			EncryptedItem item = await Load(friendlyId);
			if (item != null)
			{
				using (IDocumentSession session = _store.LightweightSession())
				{
					session.Delete(item);
					await session.SaveChangesAsync();
				}
			}
		}
	}
}