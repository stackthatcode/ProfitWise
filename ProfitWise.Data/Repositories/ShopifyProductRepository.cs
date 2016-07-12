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
            var query = @"SELECT * FROM shopifyproduct WHERE ShopId = @ShopId";
            return
                _connection
                    .Query<ShopifyProduct>(query, new { ShopId })
                    .ToList();
        }

        public virtual void Insert(ShopifyProduct product)
        {
            product.ShopId = ShopId.Value;
            var query = @"INSERT INTO shopifyproduct(ShopId, ShopifyProductId, Title) 
                        VALUES(@ShopId, @ShopifyProductId, @Title)";
            _connection.Execute(query, product);
        }

        public virtual void Delete(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyproduct 
                        WHERE ShopId = @ShopId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { ShopId, shopifyProductId });
        }
    }
}

