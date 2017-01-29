using System;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly PwShopRepository _shopDataRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;
        private readonly ShopSynchronizationService _shopSynchronizationService;

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    PwShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    MultitenantFactory factory,
                    CurrencyService currencyService, 
                    ShopSynchronizationService shopSynchronizationService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _factory = factory;
            _currencyService = currencyService;
            _shopSynchronizationService = shopSynchronizationService;
        }
        
        public int Execute(ShopifyCredentials shopifyCredentials)
        {
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(shopifyCredentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            // Map the Shop Currency Id
            var shopCurrencyId = _currencyService.AbbreviationToCurrencyId(shopFromShopify.Currency);
            _pushLogger.Info($"Shop Refresh Service for UserId : {shopifyCredentials.ShopOwnerUserId}");
            var shop = _shopDataRepository.RetrieveByUserId(shopifyCredentials.ShopOwnerUserId);

            // Update Shop with the latest
            _shopSynchronizationService.RefreshShop(shopifyCredentials.ShopOwnerUserId, shopFromShopify);
            
            // Ensure the proper Batch State exists for Shop
            var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(shop);
            var batchState = profitWiseBatchStateRepository.Retrieve();
            if (batchState == null)
            {
                var state = new PwBatchState
                {
                    PwShopId = shop.PwShopId,                    
                };

                profitWiseBatchStateRepository.Insert(state);
                _pushLogger.Info($"Created Batch State for Shop - UserId: {shop.ShopOwnerUserId}");
            }

            return shop.PwShopId;
        }
    }
}
