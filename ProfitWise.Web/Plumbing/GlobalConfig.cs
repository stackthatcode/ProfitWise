using System.Configuration;

namespace ProfitWise.Web.Plumbing
{
    public class GlobalConfig
    {
        public const string BaseUrl = "/ProfitWise";

        public static readonly string ShopifyApiKey = 
                    ConfigurationManager.AppSettings["shopify_config_apikey"];

    }
}