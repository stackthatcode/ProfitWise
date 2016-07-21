using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ProfitWise.Web.Attributes
{
    public class ShopifyApiKey
    {
        public static string Get()
        {
            return ConfigurationManager.AppSettings["shopify_config_apikey"];
        }
    }
}