using System.Threading.Tasks;

namespace Letmein.Core.Services
{
	public interface ITextEncryptionService
	{
		Task<string> StoredEncryptedJson(string json, string friendlyId, int expiresInMinutes);

		Task<EncryptedItem> LoadEncryptedJson(string friendlyId);

		Task<bool> Delete(string friendlyId);
	}
}