using System;
using System.Collections.Generic;
using System.Linq;
using Letmein.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;
using ILetmeinConfiguration = Letmein.Core.Configuration.ILetmeinConfiguration;

namespace Letmein.Tests.Unit.Core.Configuration
{
	public class DefaultConfigurationTests
	{
		public IConfigurationRoot GetConfigurationRoot(Dictionary<string, string> configDictionary)
		{
			var builder = new ConfigurationBuilder()
				.AddInMemoryCollection(configDictionary);

			return builder.Build();
		}

		[Fact]
		public void should_get_values_from_configroot_and_be_case_insensitive()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionSTRING", "connection string");
			configDictionary.Add("CLEANUP_SLEEPTIME", "60");
			configDictionary.Add("EXPIRY_TIMES", "1, 120");
			configDictionary.Add("PAGE_title", "page title");
			configDictionary.Add("HEADER_TEXT", "header text");
			configDictionary.Add("HEADER_SUBTEXT", "subtext");
			configDictionary.Add("FOOTER_TEXT", "footer");

			var configRoot = GetConfigurationRoot(configDictionary);

			// Act
			ILetmeinConfiguration config = new Letmein.Core.Configuration.Configuration(configRoot);

			// Assert
			config.PostgresConnectionString.ShouldBe("connection string");
			config.CleanupSleepTime.ShouldBe(60);
			config.ExpiryTimes.Count().ShouldBe(2);

			config.ViewConfig.ShouldNotBeNull();
			config.ViewConfig.PageTitle.ShouldBe("page title");
			config.ViewConfig.HeaderText.ShouldBe("header text");
			config.ViewConfig.HeaderSubtext.ShouldBe("subtext");
			config.ViewConfig.FooterText.ShouldBe("footer");
		}

		[Fact]
		public void should_set_default_int_values_when_under_minimum()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("CLEANUP_SLEEPTIME", "0");

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			ILetmeinConfiguration config = new Letmein.Core.Configuration.Configuration(configRoot);

			// Assert
			config.CleanupSleepTime.ShouldBe(30);
		}

		[Fact]
		public void should_parse_expiry_times()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("EXPIRY_TIMES", "1, 60, 600");

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			ILetmeinConfiguration config = new Letmein.Core.Configuration.Configuration(configRoot);

			// Assert
			var expiryTimes = config.ExpiryTimes.ToList();
			expiryTimes.Count.ShouldBe(3);
			expiryTimes[0].ShouldBe(1);
			expiryTimes[1].ShouldBe(60);
			expiryTimes[2].ShouldBe(600);
		}

		[Theory]
		[InlineData("")]
		[InlineData("this isn't a number!")]
		public void should_parse_invalid_expiry_times_and_set_default(string expiryTime)
		{
			// Arrange
			int defaultTime = (int)TimeSpan.FromHours(6).TotalMinutes;

			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("EXPIRY_TIMES", expiryTime);

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			ILetmeinConfiguration config = new Letmein.Core.Configuration.Configuration(configRoot);

			// Assert
			var expiryTimes = config.ExpiryTimes.ToList();
			expiryTimes.Count.ShouldBe(1);
			expiryTimes[0].ShouldBe(defaultTime);
		}

		[Theory]
		[InlineData("default", IdGenerationType.RandomWithProunceable)]
		[InlineData("random-with-pronounceable", IdGenerationType.RandomWithProunceable)]
		[InlineData("pronounceabLE", IdGenerationType.Prounceable)]
		[InlineData("short-PROnounceable", IdGenerationType.ShortPronounceable)]
		[InlineData("short-mixedcase", IdGenerationType.ShortMixedCase)]
		[InlineData("shortcode", IdGenerationType.ShortCode)]
		public void should_parse_idgenerationtype(string idType, IdGenerationType expectedGenerationType)
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("ID_TYPE", idType);

			var configRoot = GetConfigurationRoot(configDictionary);

			// Act
			ILetmeinConfiguration config = new Letmein.Core.Configuration.Configuration(configRoot);

			// Act
			config.IdGenerationType.ShouldBe(expectedGenerationType);
		}
	}
}