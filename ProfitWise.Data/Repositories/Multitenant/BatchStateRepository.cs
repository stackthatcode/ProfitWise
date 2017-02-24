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

        private IDbConnection Connection => _connectionWrapper.DbConn;

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }


        public PwBatchState Retrieve()
        {
            var query = @"SELECT * FROM profitwisebatchstate WHERE PwShopId = @PwShopId";
            return
                Connection
                    .Query<PwBatchState>(query, new {PwShopId = PwShop.PwShopId})
                    .FirstOrDefault();
        }

        public void Insert(PwBatchState state)
        {
            var query = @"INSERT INTO profitwisebatchstate VALUES (
                            @PwShopId, @ProductsLastUpdated, @OrderDatasetStart, 
                            @OrderDatasetEnd, @InitialRefreshJobId, @RoutineRefreshJobId)";
            Connection.Execute(query, state);
        }

        public void Update(PwBatchState state)
        {
            var query = @"UPDATE profitwisebatchstate SET 
                            ProductsLastUpdated = @ProductsLastUpdated, 
                            OrderDatasetStart = @OrderDatasetStart, 
                            OrderDatasetEnd = @OrderDatasetEnd
                            WHERE PwShopId = @PwShopId";
            Connection.Execute(query, state); 
        }

        public void UpdateInitialRefreshJobId(string initialRefreshJobId)
        {
            var query = @"UPDATE profitwisebatchstate 
                        SET InitialRefreshJobId = @initialRefreshJobId
                        WHERE PwShopId = @PwShopId";
            Connection.Execute(query, new { PwShop.PwShopId, initialRefreshJobId });
        }

        public void UpdateRoutineRefreshJobId(string routineRefreshJobId)
        {
            var query = @"UPDATE profitwisebatchstate 
                        SET RoutineRefreshJobId = @routineRefreshJobId 
                        WHERE PwShopId = @PwShopId";
            Connection.Execute(query, new { PwShop.PwShopId, routineRefreshJobId });
        }
    }
}

