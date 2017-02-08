namespace Letmein.Core.Encryption
{
    public interface ISymmetricEncryptionProvider
    {
        string Decrypt(string base64IV, string key, string encryptedTextBase64);
        string Encrypt(string base64IV, string key, string text);
        string GenerateBase64EncodedIv();
    }
}