using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HICAPSConnectLibrary.Encrypt
{ 
    /*
    * Originally Taken from MSDN Magazine and Blog site.
    * http://blogs.msdn.com/b/alejacma/archive/2008/10/23/how-to-generate-key-pairs-encrypt-and-decrypt-data-with-net-c.aspx
    * Derived Code is below.
    * All cryptographic exceptions are thrown back to the caller to handle.
    */

    /// <summary>
    /// RSA Public/Private Key Implementation
    /// 
    /// All cryptographic exceptions are thrown back to the caller to handle.
    /// RSA encryption is only meant to encrypt small blocks of data of up to
    /// 
    /// (KeySize - 384) / 8) + 7 in size.
    /// 
    /// Generating Keypairs of 4096 or greater takes a long time to generate
    /// > 30 seconds on a I3-2015
    /// </summary>
    public class RSA
    {
        private static RSACryptoServiceProvider _rsaProvider = new RSACryptoServiceProvider(new CspParameters { ProviderType = 1 });

        public RSA(){}

        public RSA(String privateKeyText)
        {
            _rsaProvider.FromXmlString(privateKeyText);
        }


        /// <summary>
        /// Generates a new KeyPair
        /// </summary>
        /// <param name="keySize"> Valid RSA Key Size: 1024,2048, 4096, 8192</param>
        /// <param name="publicKeyFileName">Filename to save Generated Key</param>
        /// <param name="privateKeyFileName">Filename to save Generated Key</param>
        public static void Keys(int keySize, string publicKeyFileName, string privateKeyFileName)
        {

                // Create a new key pair on target CSP
                var cspParams = new CspParameters
                {
                    ProviderType = 1,
                    Flags = CspProviderFlags.UseArchivableKey,
                    KeyNumber = (int) KeyNumber.Exchange
                };
             

                var rsaProvider = new RSACryptoServiceProvider(keySize, cspParams);

                File.WriteAllText(publicKeyFileName, rsaProvider.ToXmlString(false));

                // Write private/public key pair to file
                File.WriteAllText(privateKeyFileName, rsaProvider.ToXmlString(true));     

        } // Keys

        // Encrypt Text
        /// <summary>
        /// Encrypts a given string, using the given Public key.
        /// </summary>
        /// <param name="publicKeyText">XML CSP of Public Key</param>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>Encrypted Text in BASE64 Indented format</returns>
        public static string Encrypt(string publicKeyText, string plainText)
        {
            // Throw exceptions back to caller...... i.e invalid keys/etc

            var rsaProvider = new RSACryptoServiceProvider(new CspParameters { ProviderType = 1 });

            // Import public key
            rsaProvider.FromXmlString(publicKeyText);

            // Encrypt plain text
            var plainBytes = Encoding.Default.GetBytes(plainText);
            var encryptedBytes = rsaProvider.Encrypt(plainBytes, false);

            return Convert.ToBase64String(encryptedBytes);

        } // Encrypt

        // Decrypt Text
        /// <summary>
        /// Decrypts a given string, using the given Private key
        /// </summary>
        /// <param name="privateKeyText">XML CSP of Private Key</param>
        /// <param name="encryptedBase64Text">Encrypted text in Base64 format</param>
        /// <returns>Decrypted text</returns>
        public static string Decrypt(string privateKeyText, string encryptedBase64Text )
        {
            var rsaProvider = new RSACryptoServiceProvider(new CspParameters {ProviderType = 1});

            // Import private/public key pair
            rsaProvider.FromXmlString(privateKeyText);

            // Read encrypted text from file
            var encryptedBytes = Convert.FromBase64String(encryptedBase64Text);

            // Decrypt text
            var plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

            // Return Decrypted text
            return Encoding.Default.GetString(plainBytes);

        } // Decrypt

        // Decrypt Text
        /// <summary>
        /// Decrypts a given string, using the Private key stored in the _PrivateKeyText class level variable
        /// </summary>
        /// <param name="encryptedBase64Text">Encrypted text in Base64 format</param>
        /// <returns>Decrypted text</returns>
        public String Decrypt(String encryptedBase64Text)
        {
            // Read encrypted text from file
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64Text);

            // Decrypt text
            byte[] plainBytes = _rsaProvider.Decrypt(encryptedBytes, false);

            // Return Decrypted text
            return Encoding.Default.GetString(plainBytes);
        } // Decrypt

        public static int KeySize(string keyText)
        {
            var rsaProvider = new RSACryptoServiceProvider(new CspParameters { ProviderType = 1 });

            // Import private/public key pair
            rsaProvider.FromXmlString(keyText);
            return rsaProvider.KeySize;
        }
    }
}
