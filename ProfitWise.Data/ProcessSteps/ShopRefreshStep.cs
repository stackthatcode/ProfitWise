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
        private readonly PwShopRepository _shopRepository;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    PwShopRepository shopRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    MultitenantFactory factory,
                    CurrencyService currencyService)
        {
            _pushLogger = pushLogger;
            _shopRepository = shopRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _factory = factory;
            _currencyService = currencyService;
        }

        public int Execute(ShopifyCredentials shopifyCredentials)
        {
            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(shopifyCredentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            // Map the Shop Currency Id
            var shopCurrencyId = _currencyService.AbbreviationToCurrencyId(shopFromShopify.Currency);

            _pushLogger.Info($"Shop Refresh Service for UserId : {shopifyCredentials.ShopOwnerUserId}");
            var shop = _shopRepository.RetrieveByUserId(shopifyCredentials.ShopOwnerUserId);

            if (shop == null)
            {
                // Create new Shop
                var newShop = new PwShop
                {
                    UserId = shopifyCredentials.ShopOwnerUserId,
                    CurrencyId = shopCurrencyId,
                    ShopifyShopId = shopFromShopify.Id,
                    StartingDateForOrders = new DateTime(2016, 9, 1),
                };
                newShop.PwShopId = _shopRepository.Insert(newShop);

                _pushLogger.Info($"Created new Shop - UserId: {newShop.UserId}, CurrencyId: {newShop.CurrencyId}, " +
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
                _pushLogger.Info($"Updating Shop Currency - UserId: {shop.UserId}, CurrencyId: {shop.CurrencyId}");                
                _shopRepository.UpdateShopCurrency(shop);
                return shop.PwShopId;
            }
        }
    }
}
