using CloudFileStore.AWS;
using CloudFileStore.Azure;
using CloudFileStore.GoogleCloud;
using GuardNet;
using Letmein.Core;
using Letmein.Core.Configuration;
using Letmein.Core.Encryption;
using Letmein.Core.Repositories;
using Letmein.Core.Repositories.FileSystem;
using Letmein.Core.Repositories.Postgres;
using Letmein.Core.Services;
using Letmein.Core.Services.UniqueId;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Letmein.Web
{
	public class DependencyInjection
	{
		public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<IConfigurationRoot>(provider => configurationRoot);
			var configuration = new Configuration(configurationRoot);

			services.AddSingleton<ILetmeinConfiguration>(provider => configuration);
			ConfigureRepository(services, configuration, configurationRoot);

			services.AddScoped<SymmetricAlgorithm>(service => Aes.Create());
			services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();
			services.AddScoped<ISymmetricEncryptionProvider, SymmetricEncryptionProvider>();

			services.AddScoped<ITextEncryptionService, TextEncryptionService>();
		}

		private static void ConfigureRepository(IServiceCollection services, ILetmeinConfiguration configuration, IConfigurationRoot configurationRoot)
		{
			switch (configuration.RepositoryType)
			{
				case RepositoryType.FileSystem:
					break;

				case RepositoryType.S3:
					ConfigureForS3(services, configurationRoot);
					break;

				case RepositoryType.GoogleCloud:
					ConfigureForGCloud(services, configurationRoot);
					break;

				case RepositoryType.AzureBlobs:
					ConfigureForAzureBlobs(services, configurationRoot);
					break;

				case RepositoryType.Postgres:
					ConfigureForPostgres(services);
					break;

				default:
					throw new NotSupportedException("Please enter a valid repository type");
			}
		}

		private static void ConfigureForS3(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				const string sectionName = "S3";

				var s3Configuration = new S3Configuration();
				configurationRoot.Bind(sectionName, s3Configuration);
				GuardAllConfigProperties(sectionName, s3Configuration);

				var s3Provider = new S3StorageProvider(s3Configuration);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, s3Provider);
			});
		}

		private static void ConfigureForGCloud(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				const string sectionName = "GoogleCloud";

				var googleCloudConfig = new GoogleCloudConfiguration();
				configurationRoot.Bind(sectionName, googleCloudConfig);
				GuardAllConfigProperties(sectionName, googleCloudConfig);

				var googleCloudProvider = new GoogleCloudStorageProvider(googleCloudConfig);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, googleCloudProvider);
			});
		}

		private static void ConfigureForAzureBlobs(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				const string sectionName = "Azure";

				var azureConfig = new AzureBlobConfiguration();
				configurationRoot.Bind(sectionName, azureConfig);
				GuardAllConfigProperties(sectionName, azureConfig);

				var azureProvider = new AzureBlobStorageProvider(azureConfig);
				var logger = service.GetService<ILogger>();

				return new JsonTextFileRepository(logger, azureProvider);
			});
		}

		private static void GuardAllConfigProperties<T>(string sectionName, T instance)
		{
			IEnumerable<PropertyInfo> publicProperties = typeof(T).GetProperties().Where(x => x.MemberType == MemberTypes.Property);
			foreach (PropertyInfo property in publicProperties)
			{
				string value = Convert.ToString(property.GetValue(instance));
				if (string.IsNullOrEmpty(value))
					throw new ConfigurationException($"Configuration setting {sectionName}__{property.Name} is missing or empty");
			}
		}

		private static void ConfigureForPostgres(IServiceCollection services)
		{
			services.AddSingleton<IDocumentStore>(service =>
			{
				// Configure Marten
				var config = service.GetService<ILetmeinConfiguration>();
				Guard.NotNullOrEmpty(config.PostgresConnectionString, nameof(config.PostgresConnectionString));

				return DocumentStore.For(options =>
				{
					options.Connection(config.PostgresConnectionString);
					options.Schema.For<EncryptedItem>().Index(x => x.FriendlyId);
				});
			});

			services.AddScoped<ITextRepository>(service =>
			{
				var store = service.GetService<IDocumentStore>();
				return new PostgresTextRepository(store);
			});
		}
	}
}