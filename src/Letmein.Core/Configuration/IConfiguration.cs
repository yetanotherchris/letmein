﻿namespace Letmein.Core.Configuration
{
	public interface IConfiguration
	{
		string PostgresConnectionString { get; set; }
		int CleanupSleepTime { get; set; }
		int ExpirePastesAfter { get; set; }
		IdGenerationType IdGenerationType { get; set; }
		ViewConfig ViewConfig { get; set; }
	}
}