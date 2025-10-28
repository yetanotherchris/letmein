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
		private static readonly string _upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private static readonly int _upperAlphabetLength = _upperAlphabet.Length - 1;


		public string Generate(IdGenerationType idGenerationType)
		{
            /* Clash rates:
				random-with-pronounceable - 4 random characters (a-z, A-Z, 0-9), and a pronounceable password (a non-dictionary word).
					P = (1/64) * (1/64) * (1/64) * (1/64) * (1/500) (about 1 in 1 billion)

				pronounceable - 8 character pronounceable password (a non-dictionary word).
					P = (1/500) (about 1 in 500)

				short-pronounceable - 5 character pronounceable password (a non-dictionary word).
					P = (1/300) (about 1 in 300)

				short-mixedcase - 4 random characters (a-z, A-Z, 0-9).
					P = (1/64) * (1/64) * (1/64) * (1/64) (about 1 in 16 million)

				shortcode - 2 numbers, 2 characters and 2 numbers. First two numbers are the 2 from the current time's millseconds, characters are 2 uppercase, 2 digits from the current time's seconds.
					P = (1/1000) * (1/26) * (1/26) * (1/60) (about 1 in 40 million)

				bips39-two-words
					P = (1/2048) * (1/2048) (about 1 in 4 million)

				bips39-two-words-and-number:
					P = (1/2048) * (1/2048) * (1/9999) (about 1 in 40 billion)
			 */
            string id = "";

			switch (idGenerationType)
			{
                case IdGenerationType.RandomWithPronounceable:
					id = GetRandomCharacters(4) + "_" + GetPronounceable();
					break;

				case IdGenerationType.Pronounceable:
					id = GetPronounceable();
					break;

				case IdGenerationType.ShortPronounceable:
					id = GetShortPronounceable();
					break;

				case IdGenerationType.ShortMixedCase:
					id = GetRandomCharacters(4);
					break;

				case IdGenerationType.ShortCode:
					id = GetShortCode();
					break;

                case IdGenerationType.Bip39TwoWords:
                    id = Bip39Words.GetRandomWord() + "-" + Bip39Words.GetRandomWord();
                    break;

                case IdGenerationType.Bip39TwoWordsAndNumber:
                    id = Bip39Words.GetRandomWord() + "-" + Bip39Words.GetRandomWord() + "-" + Bip39Words.GetFourDigitRandomNumber();
                    break;

                default:
					throw new ArgumentOutOfRangeException(nameof(idGenerationType), idGenerationType, null);
			}

			return id;
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

		private string GetShortCode()
		{
			// e.g.: 99PB45
			char char1 = _upperAlphabet[_random.Next(0, _upperAlphabetLength)];
			char char2 = _upperAlphabet[_random.Next(0, _upperAlphabetLength)];

			string first = DateTime.Now.ToString("ff");
			string second = DateTime.Now.ToString("ss");

			return string.Format("{0}{1}{2}{3}", first, char1, char2, second);
		}
	}
}