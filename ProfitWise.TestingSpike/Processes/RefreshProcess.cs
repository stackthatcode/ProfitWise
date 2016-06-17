using System;
using ProfitWise.Batch.RefreshServices;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Batch.Processes
{
    public class RefreshProcess
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly OrderRefreshService _orderRefreshService;
        private readonly ProductRefreshService _productRefreshService;
        private readonly ILogger _logger;


        public RefreshProcess(
                ShopifyCredentialService shopifyCredentialService,
                OrderRefreshService orderRefreshService,
                ProductRefreshService productRefreshService,
                ILogger logger)
        {
            // TODO: move into Autofac configuration
           
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshService = orderRefreshService;
            _productRefreshService = productRefreshService;
            _logger = logger;
        }

        public void Execute(string userId)
        {
            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for User {userId}: {shopifyFromClaims.Message}");
            }

            var shopifyClientCredentials = new ShopifyCredentials()
            {
                ShopOwnerId = shopifyFromClaims.ShopOwnerUserId,
                ShopDomain = shopifyFromClaims.ShopDomain,
                AccessToken = shopifyFromClaims.AccessToken,
            };

            _productRefreshService.Execute(shopifyClientCredentials);
            _orderRefreshService.Execute(shopifyClientCredentials);
        }
    }
}
