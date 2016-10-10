using Autofac.Extras.DynamicProxy2;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwFilterRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }

        public PwFilterRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

    }
}

