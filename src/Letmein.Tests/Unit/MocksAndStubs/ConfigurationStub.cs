using System;
using System.Collections.Generic;
using Letmein.Core.Configuration;

namespace Letmein.Tests.Unit.MocksAndStubs
{
	public class ConfigurationStub : IConfiguration
	{
		public string PostgresConnectionString { get; set; }
		public int CleanupSleepTime { get; set; }
		public ViewConfig ViewConfig { get; set; }
		private List<int> _expiryTimes;
		public IdGenerationType IdGenerationType { get; set; }

		public IEnumerable<int> ExpiryTimes
		{
			get { return _expiryTimes; }
			set { _expiryTimes = new List<int>(value); }
		}

		public RepositoryType RepositoryType { get; set; }
		public string PastesStorePath { get; set; }

		public ConfigurationStub()
		{
			ViewConfig = new ViewConfig();
			_expiryTimes = new List<int>();
		}

		public void AddExpiryTime(int expiry)
		{
			_expiryTimes.Add(expiry);
		}
	}
}