using System;
using System.Security;

namespace Push.Utilities.Helpers
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
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DpApiDecryptString(this string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return System.Text.Encoding.Unicode.GetString(decryptedData).ToSecureString();
            }
            catch
            {
                return new SecureString();
            }
        }
    }
}
