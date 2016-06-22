using System.Linq;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;
using Dapper;

namespace ProfitWise.Data.Repositories
{
    public class ShopRepository
    {
        private readonly MySqlConnection _connection;

        public ShopRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public ShopifyShop RetrieveByShopId(int shopId)
        {
            var query = @"SELECT FROM shop WHERE ShopId = @ShopId";
            return _connection
                .Query<ShopifyShop>(query, new {@ShopId = shopId})
                .FirstOrDefault();
        }

        public ShopifyShop RetrieveByUserId(string userId)
        {
            var query = @"SELECT FROM shop WHERE UserId = @UserId";
            return _connection
                .Query<ShopifyShop>(query, new { @UserId = userId })
                .FirstOrDefault();
        }

        public int Insert(ShopifyShop shop)
        {
            var query = 
                @"INSERT INTO shop (UserId) VALUES (@UserId);
                SELECT LAST_INSERT_ID();";
            return _connection
                .Query<int>(query, new { shop })
                .First();
        }
    }
}
