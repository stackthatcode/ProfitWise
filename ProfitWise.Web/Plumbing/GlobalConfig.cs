using System.Configuration;
using System.Net;
using System.Web.Mvc;

namespace ProfitWise.Web.Plumbing
{
    public class GlobalConfig
    {
        public static readonly string BaseUrl = 
                    ConfigurationManager.AppSettings["application_root_url"];
                
        public static readonly string ShopifyApiKey = 
                    ConfigurationManager.AppSettings["shopify_config_apikey"];

        public static RedirectResult Redirect(string destinationUrl, string returnUrl = null)
        {
            var url = $"{BaseUrl}" + $"{destinationUrl}" +
                    (returnUrl != null ? "?returnUrl={WebUtility.UrlEncode(returnUrl)}" : "");
            return new RedirectResult(url);
        }
    }
}
