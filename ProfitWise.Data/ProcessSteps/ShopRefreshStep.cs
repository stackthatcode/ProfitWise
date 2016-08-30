using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.ProcessSteps
{
    public class ShopRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly PwShopRepository _shopRepository;
        private readonly MultitenantFactory _factory;

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    PwShopRepository shopRepository,
                    MultitenantFactory factory)
        {
            _pushLogger = pushLogger;
            _shopRepository = shopRepository;
            _factory = factory;
        }

        public int Execute(string userId)
        {
            _pushLogger.Info($"Shop Refresh Service for UserId : {userId}");

            var shop = _shopRepository.RetrieveByUserId(userId);

            if (shop == null)
            {
                var newShopId = _shopRepository.Insert(new PwShop {UserId = userId});

                var state = new PwBatchState
                {
                    ShopId = newShopId,                    
                };

                var newShop = _shopRepository.RetrieveByShopId(newShopId);
                var profitWiseBatchStateRepository = _factory.MakeBatchStateRepository(newShop);
                profitWiseBatchStateRepository.Insert(state);
                return newShopId;
            }
            else
            {
                return shop.PwShopId;
            }
        }
    }
}
