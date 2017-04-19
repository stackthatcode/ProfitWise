using System;
using System.Security.Cryptography;

namespace Push.Foundation.Utilities.Security
{
    public class HmacCryptoService
    {
        private readonly string _secret;

        // This is actually a bad idea - this constrains the secret to instance-level configuration
        public HmacCryptoService(string secret)
        {
            _secret = secret;
        }

        public string ToBase64EncodedSha256(string input)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(_secret);
            byte[] messageBytes = encoding.GetBytes(input);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
