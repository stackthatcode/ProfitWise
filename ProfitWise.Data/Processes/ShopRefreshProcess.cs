using System;
using System.Net;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.Processes
{
    public class ShopRefreshProcess
    {
        private readonly IShopifyCredentialService _shopifyCredentialService;
        private readonly ShopRefreshService _shopRefreshStep;
        private readonly ProductRefreshStep _productRefreshStep;
        private readonly OrderRefreshStep _orderRefreshStep;
        private readonly ProductCleanupStep _productCleanupStep;
        private readonly HangFireService _hangFireService;
        private readonly PwShopRepository _pwShopRepository;
        private readonly IPushLogger _pushLogger;


        public ShopRefreshProcess(
                IShopifyCredentialService shopifyCredentialService,
                ShopRefreshService shopRefreshStep,
                ProductRefreshStep productRefreshStep,
                OrderRefreshStep orderRefreshStep,
                ProductCleanupStep productCleanupStep,
                HangFireService hangFireService,
                IPushLogger logger, 
                PwShopRepository pwShopRepository)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshStep = orderRefreshStep;
            _productCleanupStep = productCleanupStep;
            _hangFireService = hangFireService;
            _productRefreshStep = productRefreshStep;
            _pushLogger = logger;
            _pwShopRepository = pwShopRepository;
            _shopRefreshStep = shopRefreshStep;
        }

        // TODO - add retry constraint
        public void Execute(string userId)
        {
            try
            {
                ExecuteInner(userId);
            }
            catch (Exception e)
            {
                _pushLogger.Error(e);
                throw;  // Need to do this so HangFire reschedules
            }
        }

        public void ExecuteInner(string userId)
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

            try
            {
                _shopRefreshStep.Execute(shopifyClientCredentials);
                _productRefreshStep.Execute(shopifyClientCredentials);
                _orderRefreshStep.Execute(shopifyClientCredentials);
                _productCleanupStep.Execute(shopifyClientCredentials);

                // UPDATE - that statement VVVV is not true
                // If it's already scheduled, this will only perform an update
                //_hangFireService.ScheduleRoutineShopRefresh(userId);
            }
            catch (BadHttpStatusCodeException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var shop = _pwShopRepository.RetrieveByUserId(userId);
                    _pwShopRepository.UpdateIsAccessTokenValid(shop.PwShopId, false);
                }
                throw;
            }

            _pushLogger.Info($"FIN - Refresh Process for UserId: {userId}");
        }
    }
}
