using System.Collections.Generic;
using Letmein.Core.Configuration;

namespace Letmein.Tests.Unit.MocksAndStubs
{
	public class ConfigurationStub : IConfiguration
	{
		public string PostgresConnectionString { get; set; }
		public int CleanupSleepTime { get; set; }
		public int ExpirePastesAfter { get; set; }
		public ViewConfig ViewConfig { get; set; }
		public IEnumerable<int> ExpiryTimes { get; set; }

		public ConfigurationStub()
		{
			ViewConfig = new ViewConfig();
			ExpiryTimes = new List<int>();
		}
	}
}