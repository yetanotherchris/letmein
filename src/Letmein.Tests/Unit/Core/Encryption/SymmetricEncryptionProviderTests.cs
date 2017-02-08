using System;
using System.Security.Cryptography;
using Letmein.Core.Encryption;
using NUnit.Framework;

namespace Letmein.Tests.Unit.Core.Encryption
{
	[TestFixture]
    public class SymmetricEncryptionProviderTests
    {
		[Test]
		public void should_encrypt_and_decrypt_text_when_using_different_provider_instance()
		{
            // given
		    string text = "my text";
		    string key = "mykey";

            var providerEncrypter = new SymmetricEncryptionProvider(Aes.Create());
            var providerDecrypter = new SymmetricEncryptionProvider(Aes.Create());
		    string iv = providerEncrypter.GenerateBase64EncodedIv();

		    // when
		    string encryptedText = providerEncrypter.Encrypt(iv, key, text);
		    string decryptedText = providerDecrypter.Decrypt(iv, key, encryptedText);

		    // then
            Assert.That(iv, Is.Not.Empty.Or.Null);
            Assert.That(decryptedText, Is.EqualTo(text));
		}

        [Test]
        [TestCase("", "wWAf3Xii06sSwtoenTfjAw==")]
        [TestCase("somekey", "")]
        [TestCase("somekey", "this is a bad IV")]
        public void should_throw_when_key_or_iv_is_empty_or_iv_is_invalid(string key, string iv)
        {
            // given
            string text = "my text";
            var provider = new SymmetricEncryptionProvider(Aes.Create());

            // when + then
            Assert.Throws<EncryptionException>(() => provider.Decrypt(iv, key, text));
        }

        [Test]
        public void should_throw_encryptionexception_when_key_is_wrong()
        {
            // given
            string text = "my text";
            string key = "mykey";

            var provider = new SymmetricEncryptionProvider(Aes.Create());
            string iv = provider.GenerateBase64EncodedIv();
            Console.WriteLine(iv);
            // when
            string encryptedText = provider.Encrypt(iv, key, text);

            // then
            Assert.Throws<EncryptionException>(() => provider.Decrypt(iv, "a different key", encryptedText));
        }
    }
}
