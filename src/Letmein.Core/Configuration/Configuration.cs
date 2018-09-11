using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Letmein.Core.Configuration
{
	public class Configuration : IConfiguration
	{
		public RepositoryType RepositoryType { get; set; }
		public string NotesPath { get; set; }
		public string PostgresConnectionString { get; set; }
		public int CleanupSleepTime { get; set; }
		public IdGenerationType IdGenerationType { get; set; }
		public IEnumerable<int> ExpiryTimes { get; set; }
		public ViewConfig ViewConfig { get; set; }

		public Configuration(IConfigurationRoot configRoot)
		{
			// This class needs refactoring so there's IOptions injected instead of this class.

			// Keys are case insensitive, they're just uppercase for readability/match the dockerfile.
			PostgresConnectionString = configRoot["POSTGRES_CONNECTIONSTRING"];
			NotesPath = configRoot["NOTES_PATH"];

			if (string.IsNullOrEmpty(PostgresConnectionString) || string.IsNullOrEmpty(NotesPath))
				throw new ConfigurationException("POSTGRES_CONNECTIONSTRING and NOTES_PATH are empty (keys are case insensitive). Please use one setting.");

			RepositoryType.TryParse(configRoot["RepositoryType"], true, out RepositoryType repositoryTypeParsed);
			RepositoryType = repositoryTypeParsed;

			int.TryParse(configRoot["CLEANUP_SLEEPTIME"], out var parsedSleepTime);
			CleanupSleepTime = parsedSleepTime;

			if (CleanupSleepTime < 1)
				CleanupSleepTime = 30;

			ExpiryTimes = ParseExpiryTimes(configRoot["EXPIRY_TIMES"]);
			IdGenerationType = ParseIdGenerationType(configRoot);

			ViewConfig = new ViewConfig();
			ViewConfig.PageTitle = configRoot["PAGE_TITLE"];
			ViewConfig.HeaderText = configRoot["HEADER_TEXT"];
			ViewConfig.HeaderSubtext = configRoot["HEADER_SUBTEXT"];
			ViewConfig.FooterText = configRoot["FOOTER_TEXT"];
		}

		private IdGenerationType ParseIdGenerationType(IConfigurationRoot configRoot)
		{
			IdGenerationType result = IdGenerationType.Default;

			string idTypeValue = configRoot["ID_TYPE"];
			if (!string.IsNullOrEmpty(idTypeValue))
			{
				idTypeValue = idTypeValue.ToLower();
				if (idTypeValue == "short-mixedcase")
				{
					result = IdGenerationType.ShortMixedCase;
				}
				else if (idTypeValue == "shortcode")
				{
					result = IdGenerationType.ShortCode;
				}
				else if (idTypeValue == "pronounceable")
				{
					result = IdGenerationType.Prounceable;
				}
				else if (idTypeValue == "short-pronounceable")
				{
					result = IdGenerationType.ShortPronounceable;
				}
				else
				{
					result = IdGenerationType.Default;
				}
			}

			return result;
		}

		private List<int> ParseExpiryTimes(string configValues)
		{
			var expiryTimes = new List<int>();

			if (!string.IsNullOrEmpty(configValues))
			{
				string[] values = configValues.Split(',');
				if (values.Length > 0)
				{
					expiryTimes.Clear();

					foreach (string item in values)
					{
						int expiryValue;
						if (int.TryParse(item, out expiryValue))
						{
							expiryTimes.Add(expiryValue);
						}
					}
				}
			}

			if (expiryTimes.Count == 0)
			{
				int defaultMinutes = (int)TimeSpan.FromHours(6).TotalMinutes;
				expiryTimes.Add(defaultMinutes);
			}

			return expiryTimes;
		}
	}
}