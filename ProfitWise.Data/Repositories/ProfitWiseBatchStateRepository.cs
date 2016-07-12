using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class ProfitWiseBatchStateRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;
        public int? ShopId { get; set; }

        public ProfitWiseBatchStateRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public ProfitWiseBatchState Retrieve()
        {
            var query = @"SELECT * FROM profitwisebatchstate WHERE ShopId = @ShopId";
            return
                _connection
                    .Query<ProfitWiseBatchState>(query, new {ShopId})
                    .FirstOrDefault();
        }

        public void Insert(ProfitWiseBatchState state)
        {
            var query = @"INSERT INTO profitwisebatchstate VALUES (@ShopId, @ProductsLastUpdated, @OrderDatasetStart, @OrderDatasetEnd)";
            _connection.Execute(query, state);
        }

        public void Update(ProfitWiseBatchState state)
        {
            var query = @"UPDATE profitwisebatchstate SET 
                            ProductsLastUpdated = @ProductsLastUpdated, 
                            OrderDatasetStart = @OrderDatasetStart, 
                            OrderDatasetEnd = @OrderDatasetEnd
                            WHERE ShopId = @ShopId";
            _connection.Execute(query, state);
        }
    }
}

