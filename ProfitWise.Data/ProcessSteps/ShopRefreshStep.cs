using ProfitWise.Data.Repositories;
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

        public ShopRefreshService(
                    BatchLogger pushLogger, 
                    ShopRepository shopDataRepository,
                    ApiRepositoryFactory apiRepositoryFactory,
                    ShopSynchronizationService shopSynchronizationService)
        {
            _pushLogger = pushLogger;
            _shopDataRepository = shopDataRepository;
            _apiRepositoryFactory = apiRepositoryFactory;
            _shopSynchronizationService = shopSynchronizationService;
        }
        
        public int Execute(ShopifyCredentials shopifyCredentials)
        {

            var shopApiRepository = _apiRepositoryFactory.MakeShopApiRepository(shopifyCredentials);
            var shopFromShopify = shopApiRepository.Retrieve();

            // Map the Shop Currency Id
            _pushLogger.Info($"Shop Refresh Service for Shop: {shopifyCredentials.ShopDomain}, UserId: {shopifyCredentials.ShopOwnerUserId}");
            var shop = _shopDataRepository.RetrieveByUserId(shopifyCredentials.ShopOwnerUserId);

            // Update Shop with the latest
            _shopSynchronizationService.RefreshShop(shopifyCredentials.ShopOwnerUserId, shopFromShopify);            
            return shop.PwShopId;
        }
    }
}
