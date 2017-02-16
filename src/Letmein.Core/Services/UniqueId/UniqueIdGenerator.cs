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
			var passwordBuilder = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				passwordBuilder.Append(GetUniqueChar());
			}
			passwordBuilder.Append("_");

			int passwordLength = _random.Next(5, 9);
			passwordBuilder.Append(_generator.GeneratePassword(passwordLength));

			return passwordBuilder.ToString();
		}

		private char GetUniqueChar()
		{
			int index = _random.Next(0, _alphabetLength);
			return _alphabet[index];
		}
	}
}