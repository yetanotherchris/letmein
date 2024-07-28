using System;

namespace Letmein.Core
{
	public class EncryptedItem
	{
        public Guid Id { get; set; }
		public string FriendlyId { get; set; }
        public string AlgorithmName { get; set; }
		public string CipherJson { get; set; }

	    public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset ExpiresOn { get; set; }
	}
}