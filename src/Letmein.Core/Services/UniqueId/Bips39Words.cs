using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Letmein.Core.Services.UniqueId
{
    public static class Bip39Words
    {
        private static readonly string[] Words = LoadWordlist();

        private static string[] LoadWordlist()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Letmein.Core.Services.UniqueId.english-words.txt";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetRandomWord()
        {
            int index = RandomNumberGenerator.GetInt32(0, Words.Length);
            return Words[index].Trim();
        }

        public static string GetFourDigitRandomNumber()
        {
            return RandomNumberGenerator.GetInt32(9999).ToString();
        }
    }
}
