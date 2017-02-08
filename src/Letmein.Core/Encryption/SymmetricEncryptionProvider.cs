using System;
using System.Security.Cryptography;
using System.Text;

namespace Letmein.Core.Encryption
{
    public class SymmetricEncryptionProvider : ISymmetricEncryptionProvider
    {
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private readonly UTF8Encoding _encoding;

        public SymmetricEncryptionProvider(SymmetricAlgorithm symmetricAlgorithm)
        {
            _symmetricAlgorithm = symmetricAlgorithm;
            _encoding = new UTF8Encoding();
        }

        public string GenerateBase64EncodedIv()
        {
            _symmetricAlgorithm.GenerateIV();
            return Convert.ToBase64String(_symmetricAlgorithm.IV);
        }

        public string Encrypt(string base64IV, string key, string text)
        {
            if (string.IsNullOrEmpty(key))
                throw new EncryptionException(null, "Encryption failed.", key, base64IV);

            key = EnsureKeyLength(key);

            byte[] ivBytes = Convert.FromBase64String(base64IV);
            byte[] keyBytes = _encoding.GetBytes(key);

            try
            {
                using (ICryptoTransform transform = _symmetricAlgorithm.CreateEncryptor(keyBytes, ivBytes))
                {
                    byte[] textBytes = _encoding.GetBytes(text);
                    byte[] encryptedBytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException || ex is FormatException)
            {
                throw new EncryptionException(ex, "Encryption failed. Key length: {0}, IV: {1}", key.Length, base64IV);
            }
        }

        public string Decrypt(string base64IV, string key, string encryptedTextBase64)
        {
            key = EnsureKeyLength(key);

            byte[] ivBytes = Convert.FromBase64String(base64IV);
            byte[] keyBytes = _encoding.GetBytes(key);

            try
            {
                using (ICryptoTransform transform = _symmetricAlgorithm.CreateDecryptor(keyBytes, ivBytes))
                {
                    byte[] textBytes = Convert.FromBase64String(encryptedTextBase64);
                    byte[] decryptedBytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                    return _encoding.GetString(decryptedBytes);
                }
            }
            catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException || ex is FormatException)
            {
                throw new EncryptionException(ex, "Decryption failed. Key length: {0}, IV: {1}", key.Length, base64IV);
            }
        }

        private string EnsureKeyLength(string key)
        {
            _symmetricAlgorithm.GenerateKey();
            if (key.Length > _symmetricAlgorithm.Key.Length)
            {
                // The key is too long, truncate it
                key = key.Substring(0, _symmetricAlgorithm.Key.Length - 1);
            }
            else if (key.Length < _symmetricAlgorithm.Key.Length)
            {
                // The key is too short, pad it with 0s;s
                key = key + new string('\0', _symmetricAlgorithm.Key.Length - key.Length);
            }

            return key;
        }
    }
}