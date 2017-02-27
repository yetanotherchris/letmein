using System;
using System.Collections.Generic;
using System.Linq;
using Letmein.Core.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using IConfiguration = Letmein.Core.Configuration.IConfiguration;

namespace Letmein.Tests.Unit.Core.Configuration
{
	[TestFixture]
    public class DefaultConfigurationTests
    {
	    public IConfigurationRoot GetConfigurationRoot(Dictionary<string, string> configDictionary)
	    {
		    var builder = new ConfigurationBuilder()
				.AddInMemoryCollection(configDictionary);

		    return builder.Build();
	    }

		[Test]
		public void should_get_values_from_configroot_and_be_case_insensitive()
		{
			// Arrange
			var configDictionary = new Dictionary<string,string>();
			configDictionary.Add("postgres_connectionSTRING", "connection string");
			configDictionary.Add("CLEANUP_SLEEPTIME", "60");
			configDictionary.Add("EXPIRY_TIMES", "1, 120");
			configDictionary.Add("PAGE_title", "page title");
			configDictionary.Add("HEADER_TEXT", "header text");
			configDictionary.Add("HEADER_SUBTEXT", "subtext");
			configDictionary.Add("FOOTER_TEXT", "footer");

			var configRoot = GetConfigurationRoot(configDictionary);

			// Act
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Assert
			Assert.That(config.PostgresConnectionString, Is.EqualTo("connection string"));
			Assert.That(config.CleanupSleepTime, Is.EqualTo(60));
			Assert.That(config.ExpiryTimes.Count, Is.EqualTo(2));

			Assert.That(config.ViewConfig, Is.Not.Null);
			Assert.That(config.ViewConfig.PageTitle, Is.EqualTo("page title"));
			Assert.That(config.ViewConfig.HeaderText, Is.EqualTo("header text"));
			Assert.That(config.ViewConfig.HeaderSubtext, Is.EqualTo("subtext"));
			Assert.That(config.ViewConfig.FooterText, Is.EqualTo("footer"));
		}

		[Test]
		public void should_throw_when_connection_string_is_empty()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionSTRING", "");

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);

			// Assert
			Assert.Throws<ConfigurationException>(() => new DefaultConfiguration(configRoot));
		}

		[Test]
		public void should_set_default_int_values_when_under_minimum()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionstring", "connection string");
			configDictionary.Add("CLEANUP_SLEEPTIME", "0");

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Assert
			Assert.That(config.CleanupSleepTime, Is.EqualTo(30));

		}

		[Test]
		public void should_parse_expiry_times()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionstring", "connection string");
			configDictionary.Add("EXPIRY_TIMES", "1, 60, 600");

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Assert
			var expiryTimes = config.ExpiryTimes.ToList();
			Assert.That(expiryTimes.Count, Is.EqualTo(3));
			Assert.That(expiryTimes[0], Is.EqualTo(1));
			Assert.That(expiryTimes[1], Is.EqualTo(60));
			Assert.That(expiryTimes[2], Is.EqualTo(600));
		}

		[Test]
		[TestCase("")]
		[TestCase("asdfasdf")]
		public void should_parse_invalid_expiry_times_and_set_default(string expiryTime)
		{
			// Arrange
			int defaultTime = (int) TimeSpan.FromHours(6).TotalMinutes;

			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionstring", "connection string");
			configDictionary.Add("EXPIRY_TIMES", expiryTime);

			// Act
			var configRoot = GetConfigurationRoot(configDictionary);
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Assert
			var expiryTimes = config.ExpiryTimes.ToList();
			Assert.That(expiryTimes.Count, Is.EqualTo(1));
			Assert.That(expiryTimes[0], Is.EqualTo(defaultTime));
		}

		[Test]
		[TestCase("default", IdGenerationType.RandomWithProunceable)]
		[TestCase("random-with-pronounceable", IdGenerationType.RandomWithProunceable)]
		[TestCase("pronounceabLE", IdGenerationType.Prounceable)]
		[TestCase("short-PROnounceable", IdGenerationType.ShortPronounceable)]
		[TestCase("short-mixedcase", IdGenerationType.ShortMixedCase)]
		[TestCase("shortcode", IdGenerationType.ShortCode)]
		public void should_parse_idgenerationtype(string idType, IdGenerationType expectedGenerationType)
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("POSTGRES_CONNECTIONSTRING", "notused");
			configDictionary.Add("ID_TYPE", idType);

			var configRoot = GetConfigurationRoot(configDictionary);

			// Act
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Act
			Assert.That(config.IdGenerationType, Is.EqualTo(expectedGenerationType));
		}
	}
}