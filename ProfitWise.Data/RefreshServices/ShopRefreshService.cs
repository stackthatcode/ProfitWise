using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.General;

namespace ProfitWise.Data.RefreshServices
{
    public class ShopRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopRepository _shopRepository;
        private readonly MultitenantRepositoryFactory _factory;

        public ShopRefreshService(
                    IPushLogger pushLogger, 
                    ShopRepository shopRepository,
                    MultitenantRepositoryFactory factory)
        {
            _pushLogger = pushLogger;
            _shopRepository = shopRepository;
            _factory = factory;
        }

        public int Execute(string userId)
        {
            _pushLogger.Info($"{this.ClassAndMethodName()} - for UserId : {userId}");

            var shop = _shopRepository.RetrieveByUserId(userId);

            if (shop == null)
            {
                var newShopId = _shopRepository.Insert(new ShopifyShop {UserId = userId});

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
                return shop.ShopId;
            }
        }
    }
}
