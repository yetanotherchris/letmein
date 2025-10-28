using System.Collections;
using System.Collections.Generic;

namespace Letmein.Core.Configuration
{
	public interface ILetmeinConfiguration
	{
		RepositoryType RepositoryType { get; set; }
		string PostgresConnectionString { get; set; }
		int CleanupSleepTime { get; set; }
		IdGenerationType IdGenerationType { get; set; }
		IEnumerable<int> ExpiryTimes { get; set; }
	}
}