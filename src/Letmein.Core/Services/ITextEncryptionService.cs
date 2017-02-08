namespace Letmein.Core.Services
{
    public interface ITextEncryptionService
    {
	    string StoredEncryptedJson(string json, string friendlyId);
		EncryptedItem LoadEncryptedJson(string friendlyId);
    }
}