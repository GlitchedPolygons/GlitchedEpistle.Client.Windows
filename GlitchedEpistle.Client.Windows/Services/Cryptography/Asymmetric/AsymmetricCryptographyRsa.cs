using System;
using System.Text;
using System.Security.Cryptography;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Cryptography.Asymmetric
{
    /// <summary>
    /// <see cref="IAsymmetricCryptographyRSA"/> implementation for asymmetric RSA encryption/decryption.
    /// </summary>
    /// <seealso cref="IAsymmetricCryptographyRSA" />
    public class AsymmetricCryptographyRSA : IAsymmetricCryptographyRSA
    {
        /// <inheritdoc/>
        public string Encrypt(string text, RSAParameters publicKey)
        {
            if (string.IsNullOrEmpty(text)) return null;

            byte[] encryptedData;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(text), false);
            }
            return Convert.ToBase64String(encryptedData);
        }

        /// <inheritdoc/>
        public string Decrypt(string encryptedText, RSAParameters privateKey)
        {
            if (string.IsNullOrEmpty(encryptedText)) return null;

            byte[] data;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                if (rsa.PublicOnly)
                {
                    throw new CryptographicException($"{nameof(AsymmetricCryptographyRSA)}::{nameof(Decrypt)}: You've provided a public key instead of a private key... for decryption you need the private key!");
                }
                data = rsa.Decrypt(Convert.FromBase64String(encryptedText), false);
            }
            return Encoding.UTF8.GetString(data);
        }

        /// <inheritdoc/>
        public byte[] Encrypt(byte[] data, RSAParameters publicKey)
        {
            byte[] encryptedData;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                encryptedData = rsa.Encrypt(data, false);
            }
            return encryptedData;
        }

        /// <inheritdoc/>
        public byte[] Decrypt(byte[] encryptedData, RSAParameters privateKey)
        {
            byte[] data;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(privateKey);
                if (rsa.PublicOnly)
                {
                    throw new CryptographicException($"{nameof(AsymmetricCryptographyRSA)}::{nameof(Decrypt)}: You've provided a public key instead of a private key... for decryption you need the private key!");
                }
                data = rsa.Decrypt(encryptedData, false);
            }
            return data;
        }
    }
}
