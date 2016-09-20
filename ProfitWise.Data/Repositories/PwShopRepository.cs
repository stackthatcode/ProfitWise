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

        public PwShop RetrieveByUserId(string shopOwnerUserId)
        {
            var query = @"SELECT * FROM profitwiseshop WHERE ShopOwnerUserId = @ShopOwnerUserId";
            return _connection
                .Query<PwShop>(query, new { ShopOwnerUserId = shopOwnerUserId })
                .FirstOrDefault();
        }

        public int Insert(PwShop shop)
        {
            var query =
                @"INSERT INTO profitwiseshop (ShopOwnerUserId, ShopifyShopId, CurrencyId, StartingDateForOrders, TimeZone) 
                VALUES (@ShopOwnerUserId, @ShopifyShopId, @CurrencyId, @StartingDateForOrders, @TimeZone);
                SELECT LAST_INSERT_ID();";
            return _connection
                .Query<int>(query, shop)
                .First();
        }

        public void Update(PwShop shop)
        {
            var query = @"UPDATE profitwiseshop 
                        SET CurrencyId = @CurrencyId,
                            TimeZone = @TimeZone
                        WHERE PwShopId = @PwShopId";
            _connection.Execute(query, shop);
        }
    }
}
