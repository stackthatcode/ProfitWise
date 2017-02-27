using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class BatchStateRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }

        private readonly ConnectionWrapper _connectionWrapper;
        
        public BatchStateRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }
        
        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
        }


        public PwBatchState Retrieve()
        {
            var query = @"SELECT * FROM batchstate(@PwShopId)";
            return
                _connectionWrapper
                    .Query<PwBatchState>(query, new {PwShopId = PwShop.PwShopId})
                    .FirstOrDefault();
        }

        public void Insert(PwBatchState state)
        {
            var query = @"INSERT INTO batchstate(@PwShopId) VALUES (
                            @PwShopId, @ProductsLastUpdated, @OrderDatasetStart, 
                            @OrderDatasetEnd, @InitialRefreshJobId, @RoutineRefreshJobId)";
            _connectionWrapper.Execute(query, state);
        }

        public void Update(PwBatchState state)
        {
            var query = @"UPDATE batchstate(@PwShopId) SET 
                            ProductsLastUpdated = @ProductsLastUpdated, 
                            OrderDatasetStart = @OrderDatasetStart, 
                            OrderDatasetEnd = @OrderDatasetEnd;";
            _connectionWrapper.Execute(query, state); 
        }

        public void UpdateInitialRefreshJobId(string initialRefreshJobId)
        {
            var query = @"UPDATE batchstate(@PwShopId) 
                        SET InitialRefreshJobId = @initialRefreshJobId";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId, initialRefreshJobId });
        }

        public void UpdateRoutineRefreshJobId(string routineRefreshJobId)
        {
            var query = @"UPDATE batchstate(@PwShopId) 
                        SET RoutineRefreshJobId = @routineRefreshJobId";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId, routineRefreshJobId });
        }
    }
}

