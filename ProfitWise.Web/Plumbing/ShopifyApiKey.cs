using System.Configuration;

namespace ProfitWise.Web.Plumbing
{
    public class ShopifyApiKey
    {
        public static string Get()
        {
            return ConfigurationManager.AppSettings["shopify_config_apikey"];
        }
    }
}