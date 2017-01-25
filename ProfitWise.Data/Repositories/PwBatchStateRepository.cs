using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwBatchStateRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }

        public PwBatchStateRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwBatchState Retrieve()
        {
            var query = @"SELECT * FROM profitwisebatchstate WHERE PwShopId = @PwShopId";
            return
                _connection
                    .Query<PwBatchState>(query, new {PwShopId = PwShop.PwShopId})
                    .FirstOrDefault();
        }

        public void Insert(PwBatchState state)
        {
            var query = @"INSERT INTO profitwisebatchstate VALUES (@PwShopId, @ProductsLastUpdated, @OrderDatasetStart, @OrderDatasetEnd)";
            _connection.Execute(query, state);
        }

        public void Update(PwBatchState state)
        {
            var query = @"UPDATE profitwisebatchstate SET 
                            ProductsLastUpdated = @ProductsLastUpdated, 
                            OrderDatasetStart = @OrderDatasetStart, 
                            OrderDatasetEnd = @OrderDatasetEnd
                            WHERE PwShopId = @PwShopId";
            _connection.Execute(query, state);
        }
    }
}

