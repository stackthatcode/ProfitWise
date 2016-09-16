using System;
using ProfitWise.Data.ProcessSteps;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;
using Push.Foundation.Web.Shopify;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.Processes
{
    public class RefreshProcess
    {
        private readonly IShopifyCredentialService _shopifyCredentialService;
        private readonly ShopRefreshService _shopRefreshStep;
        private readonly ProductRefreshStep _productRefreshStep;
        private readonly OrderRefreshStep _orderRefreshStep;
        private readonly ProductCleanupStep _productCleanupStep;
        private readonly IPushLogger _pushLogger;


        public RefreshProcess(
                IShopifyCredentialService shopifyCredentialService,
                ShopRefreshService shopRefreshStep,
                ProductRefreshStep productRefreshStep,
                OrderRefreshStep orderRefreshStep,
                ProductCleanupStep productCleanupStep,
                IPushLogger logger)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshStep = orderRefreshStep;
            _productCleanupStep = productCleanupStep;
            _productRefreshStep = productRefreshStep;
            _pushLogger = logger;
            _shopRefreshStep = shopRefreshStep;
        }

        public void Execute(string userId)
        {
            _pushLogger.Info($"Starting Refresh Process for UserId: {userId}");
            _pushLogger.Info($"Retrieving Shopify Credentials Claims for {userId}");
            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);

            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for User {userId}: {shopifyFromClaims.Message}");
            }

            var shopifyClientCredentials = new ShopifyCredentials()
            {
                ShopOwnerUserId = shopifyFromClaims.ShopOwnerUserId,
                ShopDomain = shopifyFromClaims.ShopDomain,
                AccessToken = shopifyFromClaims.AccessToken,
            };

            _shopRefreshStep.Execute(shopifyClientCredentials);
            _productRefreshStep.Execute(shopifyClientCredentials);
            //_orderRefreshStep.Execute(shopifyClientCredentials);
            //_productCleanupStep.Execute(shopifyClientCredentials);

            //Console.WriteLine($"Wassup {shopifyFromClaims.ShopOwnerUserId}");
            _pushLogger.Info($"FIN - Refresh Process for UserId: {userId}");
        }
    }
}
