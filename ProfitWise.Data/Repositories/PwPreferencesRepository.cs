using System;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwPreferencesRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }

        public PwPreferencesRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwPreferences Retrieve()
        {
            var query = @"SELECT * FROM profitwisepreferences WHERE PwShopId = @PwShopId";
            return _connection.Query<PwPreferences>(query, new { PwShopId = PwShop.PwShopId }).FirstOrDefault();
        }
    }
}

