using System.Collections.Generic;
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
			configDictionary.Add("EXPIRE_PASTES_AFTER", "120");
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
			Assert.That(config.ExpirePastesAfter, Is.EqualTo(120));

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

			var configRoot = GetConfigurationRoot(configDictionary);

			// Act
			Assert.Throws<ConfigurationException>(() => new DefaultConfiguration(configRoot));
		}

		[Test]
		public void should_set_default_int_values_when_under_minimum()
		{
			// Arrange
			var configDictionary = new Dictionary<string, string>();
			configDictionary.Add("postgres_connectionstring", "connection string");
			configDictionary.Add("CLEANUP_SLEEPTIME", "0");
			configDictionary.Add("EXPIRE_PASTES_AFTER", "-1");

			var configRoot = GetConfigurationRoot(configDictionary);
			IConfiguration config = new DefaultConfiguration(configRoot);

			// Act
			Assert.That(config.CleanupSleepTime, Is.EqualTo(30));
			Assert.That(config.ExpirePastesAfter, Is.EqualTo(60));

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
