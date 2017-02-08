using System;
using System.Collections.Generic;
using System.Text;

namespace Letmein.Core.Services.UniqueId.PronounceablePassword
{
	/// <summary>
	/// Generates passwords that are pronounceable. Original Java code from
	/// http://www.multicians.org/thvv/gpw.html. This class is free to use without restrictions.
	/// </summary>
	public class PronounceablePasswordGenerator
	{
		public const string Alphabet = "abcdefghijklmnopqrstuvwxyz";
		private static GpwData _data = new GpwData();
		private static readonly Random Random = new Random(); // new random source seeded by clock

		public IEnumerable<string> Generate(int passwordCount, int passwordLength)
		{
			var passwordList = new List<string>();

			for (int i = 0; i < passwordCount; i++)
			{
				string password = GeneratePassword(passwordLength);
				passwordList.Add(password);
			}

			return passwordList;
		}

		public string GeneratePassword(int passwordLength)
		{
			long sum = 0;
			int nchar = 0;
			long ranno = 0;
			double pik = 0;
			var password = new StringBuilder(passwordLength);
			pik = Random.NextDouble(); // random number [0,1]

			ranno = (long)(pik * _data.Sigma); // weight by sum of frequencies
			sum = 0;
			for (int c1 = 0; c1 < 26; c1++)
			{
				for (int c2 = 0; c2 < 26; c2++)
				{
					for (int c3 = 0; c3 < 26; c3++)
					{
						sum += _data.get_Renamed(c1, c2, c3);
						if (sum > ranno)
						{
							password.Append(Alphabet[c1]);
							password.Append(Alphabet[c2]);
							password.Append(Alphabet[c3]);
							c1 = 26; // Found start. Break all 3 loops.
							c2 = 26;
							c3 = 26;
						} // if sum
					} // for c3
				} // for c2
			} // for c1

			// Now do a random walk.
			nchar = 3;
			while (nchar < passwordLength)
			{
				int c1 = Alphabet.IndexOf((System.Char)password[nchar - 2]);
				int c2 = Alphabet.IndexOf((System.Char)password[nchar - 1]);
				sum = 0;
				for (int c3 = 0; c3 < 26; c3++)
					sum += _data.get_Renamed(c1, c2, c3);
				if (sum == 0)
				{
					break; // exit while loop
				}
				pik = Random.NextDouble();
				ranno = (long)(pik * sum);
				sum = 0;
				for (int c3 = 0; c3 < 26; c3++)
				{
					sum += _data.get_Renamed(c1, c2, c3);
					if (sum > ranno)
					{
						password.Append(Alphabet[c3]);
						c3 = 26; // break for loop
					} // if sum
				} // for c3
				nchar++;
			} // while nchar

			return password.ToString();
		}
	}
}