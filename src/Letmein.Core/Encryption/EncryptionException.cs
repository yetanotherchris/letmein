using System;

namespace Letmein.Core.Encryption
{
    public class EncryptionException : Exception
    {
        public EncryptionException(Exception innerException, string message, params object[] args) : base(string.Format(message, args), innerException)
        {
        }
    }
}