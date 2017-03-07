using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Security;

namespace ProfitWise.Data.Configuration
{
    public class ProfitWiseConfiguration : ConfigurationSection
    {
        private static readonly Hashtable _settings = 
            (Hashtable)ConfigurationManager.GetSection("profitWiseConfiguration");

        public static ProfitWiseConfiguration Settings { get; } = new ProfitWiseConfiguration();


        [ConfigurationProperty("ShopifyApiKey", IsRequired = true)]
        public string ShopifyApiKey
        {
            get { return ((string)_settings["ShopifyApiKey"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ShopifyApiKey"] = value; }
        }

        [ConfigurationProperty("ShopifyApiSecret", IsRequired = true)]
        public string ShopifyApiSecret
        {
            get { return ((string)_settings["ShopifyApiSecret"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ShopifyApiSecret"] = value; }
        }


        [ConfigurationProperty("ClaimKey", IsRequired = true)]
        public string ClaimKey
        {
            get { return ((string)_settings["ClaimKey"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ClaimKey"] = value; }
        }

        [ConfigurationProperty("ClaimIv", IsRequired = true)]
        public string ClaimIv
        {
            get { return ((string)_settings["ClaimIv"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ClaimIv"] = value; }
        }
    }
}

