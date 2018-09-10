﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Marten;

[assembly:InternalsVisibleTo("Letmein.Tests")]

namespace Letmein.Core.Repositories.Postgres
{
	public class TextRepository : ITextRepository
	{
		// docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=letmein -e POSTGRES_PASSWORD=letmein123 postgres 
		// choco install dotnetcore-runtime

		private readonly IDocumentStore _store;

		public TextRepository(IDocumentStore store)
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

		public void Save(EncryptedItem encryptedItem)
		{
			using (IDocumentSession session = _store.LightweightSession())
			{
				session.Store(encryptedItem);
				session.SaveChanges();
			}
		}

		public EncryptedItem Load(string friendlyId)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return session.Query<EncryptedItem>().FirstOrDefault(x => x.FriendlyId == friendlyId);
			}
		}

		public IEnumerable<EncryptedItem> GetExpiredItems(DateTime beforeDate)
		{
			using (IQuerySession session = _store.QuerySession())
			{
				return session.Query<EncryptedItem>().Where(x => x.ExpiresOn <= beforeDate);
			}
		}

		public void Delete(string friendlyId)
		{
			var item = Load(friendlyId);
			if (item != null)
			{
				using (IDocumentSession session = _store.LightweightSession())
				{
					session.Delete(item);
					session.SaveChanges();
				}
			}
		}
	}
}