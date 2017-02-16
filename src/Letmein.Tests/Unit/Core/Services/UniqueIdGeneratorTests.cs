using System;
using System.Collections.Generic;
using System.Linq;
using Letmein.Core.Configuration;
using Letmein.Core.Services.UniqueId;
using NUnit.Framework;

namespace Letmein.Tests.Unit.Core.Services
{
	public class UniqueIdGeneratorTests
	{
		[Test]
		[TestCase(IdGenerationType.Short)]
		[TestCase(IdGenerationType.Default)]
		[TestCase(IdGenerationType.Prounceable)]
		[TestCase(IdGenerationType.RandomWithProunceable)]
		[TestCase(IdGenerationType.ShortPronounceable)]
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
					Assert.Fail("None unique ID generated");
					break;
				}

				list.Add(password);
			}
		}

		// v1: guid, random parts in hex. Too long
		// v2: guid - parts in between. Too long, not any more random
		// v3: prounceable password and + guid. Seemed a bit overkill
		// v4: stackoverflow version. I wanted my own version
		// v5: prounceable password + SO post. Better
		// v6: prounceable password + random alphabet chars or digit

		[Test]
		[Explicit]
		[Ignore("This takes a while to run")]
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
				string pwd = generator.Generate(IdGenerationType.Prounceable);

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

		[Test]
		[Explicit]
		[Ignore("This takes 20 minutes to run")]
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
				string pwd = generator.Generate(IdGenerationType.Prounceable);

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