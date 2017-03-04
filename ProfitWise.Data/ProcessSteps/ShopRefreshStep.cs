using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly BatchLogger _pushLogger;
        private readonly ShopRepository _shopDataRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly ShopSynchronizationService _shopSynchronizationService;
        private readonly BillingService _billingService;

        public ShopRefreshService(
                    BatchLogger pushLogger, 
                    ShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    ShopSynchronizationService shopSynchronizationService, 
                    BillingService billingService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _shopSynchronizationService = shopSynchronizationService;
            _billingService = billingService;
        }
        
        public bool Execute(ShopifyCredentials credentials)
        {
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(credentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            _pushLogger.Info($"Shop Refresh Service for Shop: {credentials.ShopDomain}, UserId: {credentials.ShopOwnerUserId}");

            // Update Shop with the latest from Shopify
            _shopSynchronizationService.UpdateShop(
                credentials.ShopOwnerUserId, shopFromShopify.Currency, shopFromShopify.TimeZone);

            var shop = _shopDataRepository.RetrieveByUserId(credentials.ShopOwnerUserId);
            
            // Routine check on Shop Status
            if (shop.IsAccessTokenValid == false)
            {
                _pushLogger.Warn($"Shop {shop.PwShopId} is has an invalid Access Token");
                return false;
            }
            if (shop.IsShopEnabled == false)
            {
                _pushLogger.Warn($"Shop {shop.PwShopId} is currently disabled");
                return false;
            }

            // Routine check on Billing Status            
            var charge = _billingService.SyncAndRetrieveCurrentCharge(credentials.ShopOwnerUserId);
            
            if (charge.LastStatus == ChargeStatus.Active)
            {
                return true; // All clear, nothing to do
            }
            if (charge.LastStatus == ChargeStatus.Accepted)
            {
                _billingService.ActivateCharge(credentials.ShopOwnerUserId, charge);
                return true;
            }

            // Charge Status is neither Active nor Accepted, thus set Billing Valid to false
            _shopDataRepository.UpdateIsBillingValid(shop.PwShopId, false);
            _pushLogger.Warn($"Shop {shop.PwShopId} Shopify Charge is {charge.LastStatus}");
            return false;
        }
    }
}
