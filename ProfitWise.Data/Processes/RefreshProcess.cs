using System;
using ProfitWise.Data.RefreshServices;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;

namespace ProfitWise.Data.Processes
{
    public class RefreshProcess
    {
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly OrderRefreshService _orderRefreshService;
        private readonly ProductRefreshService _productRefreshService;
        private readonly ShopRefreshService _shopRefreshService;
        private readonly IPushLogger _pushLogger;


        public RefreshProcess(
                ShopifyCredentialService shopifyCredentialService,
                OrderRefreshService orderRefreshService,
                ProductRefreshService productRefreshService,
                ShopRefreshService shopRefreshService,
                IPushLogger logger)
        {
            // TODO: move into Autofac configuration
           
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshService = orderRefreshService;
            _productRefreshService = productRefreshService;
            _pushLogger = logger;
            _shopRefreshService = shopRefreshService;
        }

        public void Execute(string userId)
        {
            _pushLogger.Info($"{this.ClassAndMethodName()} for UserId: {userId}");

            _pushLogger.Info($"{this.ClassAndMethodName()} - retrieving Shopify Credentials Claims for {userId}");
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

            var filter = new OrderFilter()
            {
                CreatedAtMin = new DateTime(2014, 5, 1)
            };

            _shopRefreshService.Execute(userId);
            _productRefreshService.Execute(shopifyClientCredentials);

            _orderRefreshService.Execute(filter, shopifyClientCredentials);
        }
    }
}
