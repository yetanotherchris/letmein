using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Letmein.Core.Configuration
{
    public class LetmeinConfiguration : ILetmeinConfiguration
	{
        public RepositoryType RepositoryType { get; set; }
		public string PostgresConnectionString { get; set; }
        public int CleanupSleepTime { get; set; }
        public IdGenerationType IdGenerationType { get; set; }
        public IEnumerable<int> ExpiryTimes { get; set; }
        public ViewConfig ViewConfig { get; set; }
	}
}