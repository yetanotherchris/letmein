using System;

namespace Letmein.Web.Models
{
	public class EncryptedItemViewModel
	{
		public string FriendlyId { get; set; }
		public string CipherJson { get; set; }
		public DateTime ExpiryDate { get; set; }

		public TimeSpan ExpiresIn => ExpiryDate - DateTime.UtcNow;
	}
}