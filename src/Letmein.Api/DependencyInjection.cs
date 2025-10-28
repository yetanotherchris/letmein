using CloudFileStore.AWS;
using CloudFileStore.Azure;
using CloudFileStore.GoogleCloud;
using GuardNet;
using Letmein.Core;
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
using Letmein.Core.Configuration;
using Letmein.Core.Providers;
using Quartz;
using Letmein.Api.Jobs;

namespace Letmein.Api
{
    public class DependencyInjection
	{
		public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configRoot)
        {
            LetmeinConfiguration letmeinConfig = Configuration(services, configRoot);

            services.AddScoped<SymmetricAlgorithm>(service => Aes.Create());
            services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();
            services.AddScoped<ISymmetricEncryptionProvider, SymmetricEncryptionProvider>();

            ConfigureRepository(services, letmeinConfig, configRoot);
            services.AddScoped<ITextEncryptionService, TextEncryptionService>();

            ConfigureQuartz(services, configRoot);
        }

        private static LetmeinConfiguration Configuration(IServiceCollection services, IConfigurationRoot configRoot)
        {
            // ILetmeinConfiguration and ConfigurationRoot is manually injected into the DI.
            // This is because there's lots of custom parsing of env var names and values.
            var letmeinConfig = LetmeinConfigurationBuilder.Build(configRoot);
            services.AddSingleton<IConfigurationRoot>(provider => configRoot);
            services.AddSingleton<ILetmeinConfiguration>(provider => letmeinConfig);
            return letmeinConfig;
        }

        private static void ConfigureRepository(IServiceCollection services, ILetmeinConfiguration letmeinConfiguration, IConfigurationRoot configuration)
		{
			switch (letmeinConfiguration.RepositoryType)
			{
				case RepositoryType.FileSystem:
					ConfigureForFileSystem(services, configuration);
					break;

				case RepositoryType.S3:
					ConfigureForS3(services, configuration);
					break;

				case RepositoryType.GoogleCloud:
					ConfigureForGCloud(services, configuration);
					break;

				case RepositoryType.AzureBlobs:
					ConfigureForAzureBlobs(services, configuration);
					break;

				case RepositoryType.Postgres:
					ConfigureForPostgres(services);
					break;

				default:
					throw new NotSupportedException("Please enter a valid repository type");
			}
		}
		
		private static void ConfigureForFileSystem(IServiceCollection services, IConfigurationRoot configurationRoot)
		{
			services.AddSingleton<ITextRepository>(service =>
			{
				var provider = new LocalFilesystemProvider();
				var logger = service.GetService<ILogger<JsonTextFileRepository>>();

				return new JsonTextFileRepository(logger, provider);
			});
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
				var logger = service.GetService<ILogger<JsonTextFileRepository>>();

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
				var logger = service.GetService<ILogger<JsonTextFileRepository>>();

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
				var logger = service.GetService<ILogger<JsonTextFileRepository>>();

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

		private static void ConfigureQuartz(IServiceCollection services, IConfigurationRoot configRoot)
		{
			// Get cleanup schedule (supports TimeSpan format like "00:30:00" or cron expressions)
			string schedule = configRoot["CLEANUP_SCHEDULE"];

			// Default to 30 minutes if not specified
			if (string.IsNullOrWhiteSpace(schedule))
			{
				schedule = "00:30:00";
			}

			// Configure Quartz
			services.AddQuartz(q =>
			{
				var jobKey = new JobKey("NotesCleanupJob");

				q.AddJob<NotesCleanupJob>(opts => opts.WithIdentity(jobKey));

				// Try to parse as TimeSpan first
				if (TimeSpan.TryParse(schedule, out TimeSpan interval))
				{
					// Use SimpleSchedule with the TimeSpan interval
					q.AddTrigger(opts => opts
						.ForJob(jobKey)
						.WithIdentity("NotesCleanupJob-trigger")
						.WithSimpleSchedule(x => x
							.WithInterval(interval)
							.RepeatForever())
						.WithDescription($"Schedule: Every {interval}")
					);
				}
				else
				{
					// Assume it's a cron expression and let Quartz validate it
					q.AddTrigger(opts => opts
						.ForJob(jobKey)
						.WithIdentity("NotesCleanupJob-trigger")
						.WithCronSchedule(schedule)
						.WithDescription($"Schedule: {schedule}")
					);
				}
			});

			services.AddQuartzHostedService(options =>
			{
				options.WaitForJobsToComplete = true;
			});
		}
	}
}