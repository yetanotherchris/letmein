using System;
using System.Collections.Generic;
using System.Text;
using Letmein.Core.Configuration;
using Letmein.Core.Services.UniqueId.PronounceablePassword;

namespace Letmein.Core.Services.UniqueId
{
	public class UniqueIdGenerator : IUniqueIdGenerator
	{
		private static readonly PronounceablePasswordGenerator _generator = new PronounceablePasswordGenerator();
		private static readonly Random _random = new Random();
		private static readonly string _alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890";
		private static readonly int _alphabetLength = _alphabet.Length - 1;

		public string Generate(IdGenerationType idGenerationType)
		{
			string password = "";

			switch (idGenerationType)
			{
				case IdGenerationType.RandomWithProunceable:
					password = GetRandomCharacters(4) + "_" + GetPronounceable();
					break;

				case IdGenerationType.Prounceable:
					password = GetPronounceable();
					break;

				case IdGenerationType.ShortPronounceable:
					password = GetShortPronounceable();
					break;

				case IdGenerationType.Short:
					password = GetRandomCharacters(4);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(idGenerationType), idGenerationType, null);
			}

			return password;
		}

		private string GetShortPronounceable()
		{
			return _generator.GeneratePassword(5);
		}

		private string GetPronounceable()
		{
			int passwordLength = _random.Next(5, 9);
			return _generator.GeneratePassword(passwordLength);
		}

		private string GetRandomCharacters(int length)
		{
			var builder = new StringBuilder();

			for (int i = 0; i < length; i++)
			{
				builder.Append(GetUniqueChar());
			}

			return builder.ToString();
		}

		private char GetUniqueChar()
		{
			int index = _random.Next(0, _alphabetLength);
			return _alphabet[index];
		}
	}
}