using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Push.Utilities.Helpers;

namespace Push.Utilities.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string input);
        string Decrypt(string input);
    }

    public class EncryptionService : IEncryptionService
    {
        public string Key { get; set; }
        public string IV { get; set; }

        public EncryptionService(string key, string iv)
        {
            Key = key;
            IV = iv;
        }

        public string Encrypt(string input)
        {
            if (Key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (IV == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesEncryptString(Key, IV);
        }

        public string Decrypt(string input)
        {
            if (Key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (IV == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesDecryptString(Key, IV);
        }
    }
}
