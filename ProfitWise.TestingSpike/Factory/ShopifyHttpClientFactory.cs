using System;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Batch.Factory
{
    public interface IShopifyClientFactory
    {
        IShopifyHttpClient Make(string userId);
    }


    public class ShopifyHttpClientFactory : IShopifyClientFactory
    {
        private readonly ILogger _logger;

        public ShopifyHttpClientFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IShopifyHttpClient Make(string userId)
        {            
            var context = ApplicationDbContext.Create();
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var shopifyCredentialService = new ShopifyCredentialService(userManager);
            var shopifyFromClaims = shopifyCredentialService.Retrieve(userId);

            // TODO - notify the Alerting Service on this type of failure
            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(shopifyFromClaims.Message);
            }

            var httpClient = new HttpClient();
            var shopifyClient =
                new ShopifyHttpClient(httpClient, _logger, shopifyFromClaims.ShopDomain, shopifyFromClaims.AccessToken);

            // Load configuration values
            if (ConfigurationManager.AppSettings["ShopifyRetryLimit"] != null)
                shopifyClient.ShopifyRetryLimit = Int32.Parse(ConfigurationManager.AppSettings["ShopifyRetryLimit"]);
            if (ConfigurationManager.AppSettings["ShopifyHttpTimeout"] != null)
                shopifyClient.ShopifyHttpTimeout = Int32.Parse(ConfigurationManager.AppSettings["ShopifyHttpTimeout"]);
            if (ConfigurationManager.AppSettings["ShopifyThrottlingDelay"] != null)
                shopifyClient.ShopifyThrottlingDelay = Int32.Parse(ConfigurationManager.AppSettings["ShopifyThrottlingDelay"]);

            return shopifyClient;
        }
    }
}
