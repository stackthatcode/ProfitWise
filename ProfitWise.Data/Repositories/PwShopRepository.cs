using System.Linq;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;
using Dapper;

namespace ProfitWise.Data.Repositories
{
    public class PwShopRepository
    {
        private readonly MySqlConnection _connection;

        public PwShopRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwShop RetrieveByShopId(int pwShopId)
        {
            var query = @"SELECT * FROM profitwiseshop WHERE PwShopId = @PwShopId";
            return _connection
                .Query<PwShop>(query, new {@PwShopId = pwShopId })
                .FirstOrDefault();
        }

        public PwShop RetrieveByUserId(string userId)
        {
            var query = @"SELECT * FROM profitwiseshop WHERE UserId = @UserId";
            return _connection
                .Query<PwShop>(query, new { @UserId = userId })
                .FirstOrDefault();
        }

        public int Insert(PwShop shop)
        {
            var query =
                @"INSERT INTO profitwiseshop (UserId) VALUES (@UserId);
                SELECT LAST_INSERT_ID();";
            return _connection
                .Query<int>(query, shop)
                .First();
        }
    }
}
