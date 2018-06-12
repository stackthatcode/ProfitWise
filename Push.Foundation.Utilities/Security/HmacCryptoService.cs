using System;
using System.Security.Cryptography;
using System.Text;

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

        public string ToHexStringSha256(string input)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(_secret);
            byte[] messageBytes = encoding.GetBytes(input);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                
                return ByteArrayToString(hashmessage);
            }
        }

        public string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
