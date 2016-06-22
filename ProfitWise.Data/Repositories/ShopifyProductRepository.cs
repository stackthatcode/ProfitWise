using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;


namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class ShopifyProductRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;

        public int? ShopId { get; set; }

        public ShopifyProductRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public virtual IList<ShopifyProduct> RetrieveAll()
        {
            var query = @"SELECT * FROM shopifyproduct WHERE ShopId";
            return
                _connection
                    .Query<ShopifyProduct>(query, new { ShopId.Value })
                    .ToList();
        }

        public virtual void Insert(ShopifyProduct product)
        {
            product.ShopId = ShopId.Value;
            var query = @"INSERT INTO shopifyproduct(UserId, ShopifyProductId, Title) 
                        VALUES(@UserId, @ShopifyProductId, @Title)";
            _connection.Execute(query, product);
        }

        public virtual void Delete(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyproduct 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { ShopId.Value, shopifyProductId });
        }
    }
}

