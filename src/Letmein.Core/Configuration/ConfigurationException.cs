using System;

namespace Letmein.Core.Configuration
{
	public class ConfigurationException : Exception
	{
		public ConfigurationException(string message) : base(message)
		{
		}
	}
}