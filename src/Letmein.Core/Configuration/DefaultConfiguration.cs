using Microsoft.Extensions.Configuration;

namespace Letmein.Core.Configuration
{
	public class DefaultConfiguration : IConfiguration
	{
		public string PostgresConnectionString { get; set; }
		public int CleanupSleepTime { get; set; }
		public int ExpirePastesAfter { get; set; }
		public ViewConfig ViewConfig { get; set; }

		public DefaultConfiguration(IConfigurationRoot configRoot)
		{
			// Keys are case insensitive, they're just uppercase for readability/match the dockerfile.
			PostgresConnectionString = configRoot.GetValue<string>("POSTGRES_CONNECTIONSTRING");
			if (string.IsNullOrEmpty(PostgresConnectionString))
				throw new ConfigurationException("POSTGRES_CONNECTIONSTRING variable is empty (keys are case insensitive).");

			CleanupSleepTime = configRoot.GetValue<int>("CLEANUP_SLEEPTIME");
			if (CleanupSleepTime < 1)
				CleanupSleepTime = 30;

			ExpirePastesAfter = configRoot.GetValue<int>("EXPIRE_PASTES_AFTER");
			if (ExpirePastesAfter < 1)
				ExpirePastesAfter = 60;

			ViewConfig = new ViewConfig();
			ViewConfig.PageTitle = configRoot.GetValue<string>("PAGE_TITLE");
			ViewConfig.HeaderText = configRoot.GetValue<string>("HEADER_TEXT");
			ViewConfig.HeaderSubtext = configRoot.GetValue<string>("HEADER_SUBTEXT");
			ViewConfig.FooterText = configRoot.GetValue<string>("FOOTER_TEXT");
		}
	}
}