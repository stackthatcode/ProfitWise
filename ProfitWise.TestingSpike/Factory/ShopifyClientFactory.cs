using System;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Shopify.HttpClient;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch.Factory
{
    public class ShopifyClientFactory
    {
        public static ShopifyHttpClient3 Make(string userId)
        {
            var apiKey = ConfigurationManager.AppSettings["shopify_config_apikey"];
            var apiSecret = ConfigurationManager.AppSettings["shopify_config_apisecret"];

            var context = ApplicationDbContext.Create();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var shopifyCredentialService = new ShopifyCredentialService(userManager);
            var shopifyFromClaims = shopifyCredentialService.Retrieve(userId);

            // TODO - notify the Alerting Service on this type of failure
            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(shopifyFromClaims.Message);
            }

            var shopifyClient =
                ShopifyHttpClient3.Factory(apiKey, apiSecret, shopifyFromClaims.ShopName, shopifyFromClaims.AccessToken);

            return shopifyClient;
        }
    }
}
