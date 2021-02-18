using System;
using System.Collections.Generic;
using System.Linq;
using Letmein.Core.Configuration;
using Letmein.Core.Services.UniqueId;
using Xunit;

namespace Letmein.Tests.Unit.Core.Services
{
	public class UniqueIdGeneratorTests
	{
		[Theory]
		[InlineData(IdGenerationType.ShortMixedCase)]
		[InlineData(IdGenerationType.ShortCode)]
		[InlineData(IdGenerationType.Pronounceable)]
		[InlineData(IdGenerationType.RandomWithPronounceable)]
		[InlineData(IdGenerationType.ShortPronounceable)]
		public void should_generate_unqueids(IdGenerationType idGenerationType)
		{
			// Arrange
			var generator = new UniqueIdGenerator();
			var list = new List<string>();

			// Act + Assert
			for (int i = 0; i < 5; i++)
			{
				string password = generator.Generate(idGenerationType);
				Console.WriteLine(password);

				if (list.Contains(password))
				{
					throw new Exception("None unique ID generated");
				}

				list.Add(password);
			}
		}

		[Fact(Skip = "This takes a while to run")]
		public void clash_test2()
		{
			// Arrange

			var generator = new UniqueIdGenerator();
			var list = new List<string>();
			DateTime now = DateTime.Now;

			// Act
			int i = 0;
			while (i < 100000)
			{
				string pwd = generator.Generate(IdGenerationType.Pronounceable);

				if (list.Contains(pwd))
				{
					Console.WriteLine("{0} - {1}", i, pwd);
					break;
				}

				list.Add(pwd);
				i++;
			}

			// Assert
			Console.WriteLine(list.Last());
			Console.WriteLine(list.Count);
			Console.WriteLine(list.Distinct().Count());
		}

		[Fact(Skip = "This takes 20 minutes to run")]
		public void clash_test()
		{
			// Arrange
			int secondsToRun = 60 * 20;

			var generator = new UniqueIdGenerator();
			var list = new List<string>();
			DateTime now = DateTime.Now;

			// Act
			int i = 0;
			TimeSpan total = DateTime.Now - now;
			while (total.TotalSeconds < secondsToRun)
			{
				string pwd = generator.Generate(IdGenerationType.Pronounceable);

				if (list.Contains(pwd))
					break;

				if (i % 1000 == 0)
				{
					System.Diagnostics.Debug.WriteLine(i);
				}

				list.Add(pwd);
				i++;

				total = DateTime.Now - now;
			}

			// Assert
			Console.WriteLine(list.Last());
			Console.WriteLine(list.Count);
			Console.WriteLine(list.Distinct().Count());
		}
	}
}