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

            shop.CurrencyId = shopCurrencyId;
            shop.TimeZone = shopFromShopify.TimeZone;
            _shopDataRepository.Update(shop);
            _pushLogger.Info(
                    $"Updated Shop - UserId: {shop.ShopOwnerUserId}, " +
                    $"CurrencyId: {shop.CurrencyId}, " +
                    $"TimeZone: {shop.TimeZone}");

            return shop.PwShopId;
        }
    }
}
