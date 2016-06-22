using System;
using ProfitWise.Data.RefreshServices;
using Push.Foundation.Web.Identity;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;

namespace ProfitWise.Data.Processes
{
    public class RefreshProcess
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly OrderRefreshService _orderRefreshService;
        private readonly ProductRefreshService _productRefreshService;
        private readonly IPushLogger _pushLogger;


        public RefreshProcess(
                ShopifyCredentialService shopifyCredentialService,
                OrderRefreshService orderRefreshService,
                ProductRefreshService productRefreshService,
                IPushLogger logger)
        {
            // TODO: move into Autofac configuration
           
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshService = orderRefreshService;
            _productRefreshService = productRefreshService;
            _pushLogger = logger;
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
