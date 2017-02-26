namespace Letmein.Core.Services
{
    public interface ITextEncryptionService
    {
	    string StoredEncryptedJson(string json, string friendlyId, int expiresInMinutes);
		EncryptedItem LoadEncryptedJson(string friendlyId);
	    bool Delete(string friendlyId);
    }
}