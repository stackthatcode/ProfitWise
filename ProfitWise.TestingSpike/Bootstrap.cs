using System.Configuration;
using Push.Utilities.Logging;
using Push.Utilities.Security;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch
{
    public class Bootstrap
    {
        public static void ConfigureApp()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("ProfitWise.Batch");
            var encryption_key = ConfigurationManager.AppSettings["security_aes_key"];
            var encryption_iv = ConfigurationManager.AppSettings["security_aes_iv"];

            var crypto_service = new EncryptionService(encryption_key, encryption_iv);
            ShopifyCredentialService.EncryptionService = crypto_service;
        }
    }
}
