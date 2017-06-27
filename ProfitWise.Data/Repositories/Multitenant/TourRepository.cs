using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class TourRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }

        private readonly ConnectionWrapper _connectionWrapper;

        public TourRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public void Insert()
        {
            var query = @"INSERT INTO tour(@PwShopId) VALUES (
                            @PwShopId, 1, 1, 1, 1,  1, 1, 1, 1 )";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId });
        }

        public PwTourState Retreive()
        {
            var query = @"SELECT * FROM tour(@PwShopId);";
            return _connectionWrapper
                    .Query<PwTourState>(query, new { PwShop.PwShopId })
                    .FirstOrDefault();
        }

        public void Update(PwTourState tour)
        {
            var query = @"UPDATE tour(@PwShopId) SET
                        ShowPreferences = @ShowPreferences,
                        ShowProducts = @ShowProducts,
                        ShowProductDetails = @ShowProductDetails,
                        ShowProductConsolidationOne = @ShowProductConsolidationOne,
                        ShowProductConsolidationTwo = @ShowProductConsolidationTwo,
                        ShowProfitabilityDashboard = @ShowProfitabilityDashboard,
                        ShowEditFilters	= @ShowEditFilters,
                        ShowProfitabilityDetail	= @ShowProfitabilityDetail,
                        ShowGoodsOnHand = @ShowGoodsOnHand ";
            _connectionWrapper.Execute(query, tour);
        }
    }
}

