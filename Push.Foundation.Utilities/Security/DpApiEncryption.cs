using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Push.Foundation.Utilities.Security
{
    public static class DpApiCrypto
    {
        // TODO: move the Salt somewhere else, tooo!
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("Salt My Boomba, Baby!");

        public static string DpApiEncryptString(this SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(input.ToInsecureString()),
                entropy,
                System.Security.Cryptography.DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DpApiDecryptString(this string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);
                return System.Text.Encoding.Unicode.GetString(decryptedData).ToSecureString();
            }
            catch (Exception ex)
            {
                return new SecureString();
            }
        }

        public static string AesEncryptString(this string input, string keyString, string IVString)
        {
            var Key = Encoding.UTF8.GetBytes(keyString);
            var IV = Encoding.UTF8.GetBytes(IVString);
            byte[] rawPlaintext = System.Text.Encoding.Unicode.GetBytes(input);

            // Check arguments.
            if (input == null || input.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (Aes aes = new AesManaged())
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
                aes.IV = IV;

                byte[] cipherText = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(rawPlaintext, 0, rawPlaintext.Length);
                    }

                    cipherText = ms.ToArray();
                }

                // Return the encrypted bytes from the memory stream.
                return Convert.ToBase64String(cipherText);
            }
        }


        public static string AesDecryptString(this string encryptedText, string keyString, string IVString)
        {
            var Key = Encoding.UTF8.GetBytes(keyString);
            var IV = Encoding.UTF8.GetBytes(IVString);
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            byte[] plainText = null;

            using (Aes aes = new AesManaged())
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherText, 0, cipherText.Length);
                    }

                    plainText = ms.ToArray();
                }
                string s = Encoding.Unicode.GetString(plainText);
                return s;
            }
        }
    }
}
    