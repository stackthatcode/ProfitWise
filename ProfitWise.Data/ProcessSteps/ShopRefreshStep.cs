using ProfitWise.Data.Repositories;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Web.Interfaces;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly BatchLogger _pushLogger;
        private readonly ShopRepository _shopDataRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly ShopSynchronizationService _shopSynchronizationService;
        private readonly CurrencyService _currencyService;

        public ShopRefreshService(
                    BatchLogger pushLogger, 
                    ShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    ShopSynchronizationService shopSynchronizationService, 
                    CurrencyService currencyService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _shopSynchronizationService = shopSynchronizationService;
            _currencyService = currencyService;
        }
        
        public void Execute(ShopifyCredentials shopifyCredentials)
        {
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(shopifyCredentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            _pushLogger.Info($"Shop Refresh Service for Shop: {shopifyCredentials.ShopDomain}, UserId: {shopifyCredentials.ShopOwnerUserId}");

            // Update Shop with the latest
            _shopSynchronizationService.UpdateShop(
                shopifyCredentials.ShopOwnerUserId, shopFromShopify.Currency, shopFromShopify.TimeZone);

            // TODO - add Billing Check
        }
    }
}
