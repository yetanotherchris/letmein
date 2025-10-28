using System;

namespace Letmein.Api.Models
{
	public class EncryptedItemModel
	{
		public string FriendlyId { get; set; }
		public string CipherJson { get; set; }
		public DateTimeOffset ExpiryDate { get; set; }
	}
}