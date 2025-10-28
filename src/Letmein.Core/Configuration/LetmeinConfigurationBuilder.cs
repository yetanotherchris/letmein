using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Letmein.Core.Configuration
{
    public class LetmeinConfigurationBuilder
    { 
        public static LetmeinConfiguration Build(IConfigurationRoot configRoot)
        {
            var config = new LetmeinConfiguration();

            config.PostgresConnectionString = configRoot["POSTGRES_CONNECTIONSTRING"];

            RepositoryType.TryParse(configRoot["REPOSITORY_TYPE"], false, out RepositoryType repositoryTypeParsed);
            config.RepositoryType = repositoryTypeParsed;

            int.TryParse(configRoot["CLEANUP_SLEEPTIME"], out var parsedSleepTime);
            config.CleanupSleepTime = parsedSleepTime;

            if (config.CleanupSleepTime < 1)
                config.CleanupSleepTime = 30;

            config.ExpiryTimes = ParseExpiryTimes(configRoot["EXPIRY_TIMES"]);
            config.IdGenerationType = ParseIdGenerationType(configRoot);


            return config;
        }

        private static IdGenerationType ParseIdGenerationType(IConfigurationRoot configRoot)
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
                else if (idTypeValue == "short-pronounceable")
                {
                    result = IdGenerationType.ShortPronounceable;
                }
                else if (idTypeValue == "pronounceable")
                {
                    result = IdGenerationType.Pronounceable;
                }
                else if (idTypeValue == "random-with-pronounceable")
                {
                    result = IdGenerationType.RandomWithPronounceable;
                }
                else if (idTypeValue == "bips39-two-words")
                {
                    result = IdGenerationType.Bip39TwoWords;
                }
                else if (idTypeValue == "bips39-two-words-and-number")
                {
                    result = IdGenerationType.Bip39TwoWordsAndNumber;
                }
                else
                {
                    result = IdGenerationType.Default;
                }
            }

            return result;
        }

        private static List<int> ParseExpiryTimes(string configValues)
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