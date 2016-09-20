using System;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
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

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    PwShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    MultitenantFactory factory,
                    CurrencyService currencyService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _factory = factory;
            _currencyService = currencyService;
        }


        // TODO => update before going to Production
        public readonly DateTime DefaultStartDateForOrders = new DateTime(2016, 9, 1);


        public int Execute(ShopifyCredentials shopifyCredentials)
        {
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(shopifyCredentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            // Map the Shop Currency Id
            var shopCurrencyId = _currencyService.AbbreviationToCurrencyId(shopFromShopify.Currency);

            _pushLogger.Info($"Shop Refresh Service for UserId : {shopifyCredentials.ShopOwnerUserId}");
            var shop = _shopDataRepository.RetrieveByUserId(shopifyCredentials.ShopOwnerUserId);
            
            if (shop == null)
            {
                // Create new Shop
                var newShop = new PwShop
                {
                    ShopOwnerUserId = shopifyCredentials.ShopOwnerUserId,
                    CurrencyId = shopCurrencyId,
                    ShopifyShopId = shopFromShopify.Id,
                    StartingDateForOrders = DefaultStartDateForOrders, 
                    TimeZone = shopFromShopify.TimeZone,
                };
                newShop.PwShopId = _shopDataRepository.Insert(newShop);

                _pushLogger.Info($"Created new Shop - UserId: {newShop.ShopOwnerUserId}, CurrencyId: {newShop.CurrencyId}, " +
                            $"ShopifyShopId: {newShop.ShopifyShopId}, StartingDateForOrders: {newShop.StartingDateForOrders}");
                
                // Add the Batch State
                var state = new PwBatchState
                {
                    PwShopId = newShop.PwShopId,                    
                };

                var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
                profitWiseBatchStateRepository.Insert(state);
                return newShop.PwShopId;
            }
            else
            {
                shop.CurrencyId = shopCurrencyId;
                shop.TimeZone = shopFromShopify.TimeZone;
                _pushLogger.Info($"Updating Shop - UserId: " +
                        $"{shop.ShopOwnerUserId}, CurrencyId: {shop.CurrencyId}, TimeZone: {shop.TimeZone}");
                _shopDataRepository.Update(shop);
                return shop.PwShopId;
            }
        }
    }
}
