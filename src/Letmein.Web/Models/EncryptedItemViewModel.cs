using System;

namespace Letmein.Web.Models
{
	public class EncryptedItemViewModel
	{
		public string FriendlyId { get; set; }
		public string CipherJson { get; set; }
		public DateTimeOffset ExpiryDate { get; set; }
	}
}