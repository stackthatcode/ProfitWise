using System;
using System.Net;
using Hangfire;
using ProfitWise.Data.Database;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Utility;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Interfaces;


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
        private readonly ShopRepository _pwShopRepository;
        private readonly ConnectionWrapper _connectionWrapper;
        private readonly BatchLogger _pushLogger;


        public ShopRefreshProcess(
                IShopifyCredentialService shopifyCredentialService,
                ShopRefreshService shopRefreshStep,
                ProductRefreshStep productRefreshStep,
                OrderRefreshStep orderRefreshStep,
                ProductCleanupStep productCleanupStep,
                HangFireService hangFireService,
                BatchLogger logger, 
                ShopRepository pwShopRepository,
                ConnectionWrapper connectionWrapper)
        {
            _shopifyCredentialService = shopifyCredentialService;
            _orderRefreshStep = orderRefreshStep;
            _productCleanupStep = productCleanupStep;
            _hangFireService = hangFireService;
            _productRefreshStep = productRefreshStep;
            _pushLogger = logger;
            _pwShopRepository = pwShopRepository;
            _shopRefreshStep = shopRefreshStep;
            _connectionWrapper = connectionWrapper;
        }
        

        [AutomaticRetry(Attempts = 1)]
        [Queue(ProfitWiseQueues.InitialShopRefresh)]
        public void InitialShopRefresh(string userId)
        {
            try
            {
                ExecuteInner(userId);
            }
            catch (Exception e)
            {
                _pushLogger.Error($"InitialShopRefresh failure for User Id: {userId}");
                _pushLogger.Error(e);
                throw; // Need to do this so HangFire reschedules
            }
            finally
            {
                // If it's already scheduled, this will only perform an update
                _hangFireService.AddOrUpdateRoutineShopRefresh(userId);
            }
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue(ProfitWiseQueues.RoutineShopRefresh)]
        public void RoutineShopRefresh(string userId)
        {
            try
            {
                ExecuteInner(userId);
            }
            catch (Exception e)
            {
                _pushLogger.Error($"RoutineShopRefresh failure for User Id: {userId}");
                _pushLogger.Error(e);
                throw;  // Need to do this so HangFire reschedules
            }
        }

        private void ExecuteInner(string userId)
        {
            var id = _connectionWrapper.Identifier;
            _pushLogger.Debug($"Connection Wrapper Id: {id}");

            var shopifyFromClaims = _shopifyCredentialService.Retrieve(userId);
            if (shopifyFromClaims.Success == false)
            {
                throw new Exception(
                    $"ShopifyCredentialService unable to Retrieve for Shop: " +
                    $"{shopifyFromClaims.ShopDomain}, UserId: {userId} - {shopifyFromClaims.Message}");
            }

            var credentials = shopifyFromClaims.ToShopifyCredentials();            
            _pushLogger.Info($"Starting Refresh Process for Shop: {credentials.ShopDomain}, UserId: {userId}");
            _pushLogger.Debug($"Retrieving Shopify Credentials Claims for Shop: {credentials.ShopDomain}, UserId: {userId}");

            try
            {
                if (!_shopRefreshStep.Execute(credentials))
                {
                    return;
                }

                _productRefreshStep.Execute(credentials);
                _orderRefreshStep.Execute(credentials);
                _productCleanupStep.Execute(credentials);

                // Change the store's status...
                var shop = _pwShopRepository.RetrieveByUserId(credentials.ShopOwnerUserId);
                _pwShopRepository.UpdateIsDataLoaded(shop.PwShopId, true);
            }
            catch (BadHttpStatusCodeException exception)
            {
                if (exception.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var shop = _pwShopRepository.RetrieveByUserId(userId);
                    _pwShopRepository.UpdateIsAccessTokenValid(shop.PwShopId, false);
                    _pushLogger.Info($"Access Token is no longer valid for Shop {shop.PwShopId}");
                }
            }

            _pushLogger.Info($"FIN - Refresh Process for Shop: {credentials.ShopDomain}, UserId: {userId}");
        }
    }
}
