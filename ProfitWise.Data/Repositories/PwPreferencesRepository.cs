using System;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class PwPreferencesRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;
        public int? PwShopId { get; set; }

        public PwPreferencesRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwPreferences Retrieve()
        {
            var query = @"SELECT * FROM profitwisepreferences WHERE ShopId = @ShopId";
            return _connection.Query<PwPreferences>(query, new {ShopId = PwShopId}).FirstOrDefault();
        }
    }
}

