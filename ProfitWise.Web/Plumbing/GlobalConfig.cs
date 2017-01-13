using System.Configuration;

namespace ProfitWise.Web.Plumbing
{
    public class GlobalConfig
    {
        public static readonly string BaseUrl = 
                    ConfigurationManager.AppSettings["application_root_url"];
                
        public static readonly string ShopifyApiKey = 
                    ConfigurationManager.AppSettings["shopify_config_apikey"];
    }
}
