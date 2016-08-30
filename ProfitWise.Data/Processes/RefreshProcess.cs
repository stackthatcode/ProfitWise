using System;
using ProfitWise.Data.Steps;
using Push.Foundation.Utilities.General;
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
        private readonly OrderRefreshService _orderRefreshStep;
        private readonly ProductRefreshStep _productRefreshStep;
        private readonly ShopRefreshService _shopRefreshStep;
        private readonly IPushLogger _pushLogger;


        public RefreshProcess(
                ShopifyCredentialService shopifyCredentialService,
                OrderRefreshService orderRefreshStep,
                ProductRefreshStep productRefreshStep,
                ShopRefreshService shopRefreshStep,
                IPushLogger logger)
        {
            // TODO: move into Autofac configuration
           
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshStep = orderRefreshStep;
            _productRefreshStep = productRefreshStep;
            _pushLogger = logger;
            _shopRefreshStep = shopRefreshStep;
        }

        public void Execute(string userId)
        {
            _pushLogger.Info($"Refresh Process for UserId: {userId}");

            _pushLogger.Info($"Retrieving Shopify Credentials Claims for {userId}");
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

            _shopRefreshStep.Execute(userId);
            _productRefreshStep.Execute(shopifyClientCredentials);
            _orderRefreshStep.Execute(shopifyClientCredentials);
        }
    }
}
