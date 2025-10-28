using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Letmein.Api;
using Letmein.Core.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Letmein.Tests.Integration
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _testDataPath;

        public ApiWebApplicationFactory()
        {
            // Create a unique test data directory for each test run
            _testDataPath = Path.Combine(Path.GetTempPath(), "letmein-tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDataPath);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add in-memory configuration for testing with high priority
                var testConfig = new Dictionary<string, string>
                {
                    ["REPOSITORY_TYPE"] = "FileSystem",
                    ["EXPIRY_TIMES"] = "60,360,1440", // 1 hour, 6 hours, 1 day
                    ["CLEANUP_SCHEDULE"] = "01:00:00", // 1 hour (won't actually run in tests)
                    ["DATA_PATH"] = _testDataPath
                };

                config.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureServices((context, services) =>
            {
                // Override the configuration to ensure our test config is used
                var testConfigRoot = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["REPOSITORY_TYPE"] = "FileSystem",
                        ["EXPIRY_TIMES"] = "60,360,1440",
                        ["CLEANUP_SCHEDULE"] = "01:00:00",
                        ["DATA_PATH"] = _testDataPath
                    })
                    .Build();

                // Remove existing ILetmeinConfiguration and IConfigurationRoot registrations
                var letmeinConfigDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILetmeinConfiguration));
                if (letmeinConfigDescriptor != null)
                {
                    services.Remove(letmeinConfigDescriptor);
                }

                var configRootDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfigurationRoot));
                if (configRootDescriptor != null)
                {
                    services.Remove(configRootDescriptor);
                }

                // Add test configuration
                var letmeinConfig = LetmeinConfigurationBuilder.Build(testConfigRoot);
                services.AddSingleton<IConfigurationRoot>(testConfigRoot);
                services.AddSingleton<ILetmeinConfiguration>(letmeinConfig);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up test data directory
                try
                {
                    if (Directory.Exists(_testDataPath))
                    {
                        Directory.Delete(_testDataPath, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            base.Dispose(disposing);
        }
    }
}
