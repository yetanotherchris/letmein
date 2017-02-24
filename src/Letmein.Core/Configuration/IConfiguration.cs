using System.Collections;
using System.Collections.Generic;

namespace Letmein.Core.Configuration
{
	public interface IConfiguration
	{
		string PostgresConnectionString { get; set; }
		int CleanupSleepTime { get; set; }
		int ExpirePastesAfter { get; set; }
		ViewConfig ViewConfig { get; set; }
		IEnumerable<int> ExpiryTimes { get; set; }
	}
}