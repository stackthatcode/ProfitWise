using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    public class ShopifyVariantRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;
        public int? ShopId { get; set; }

        public ShopifyVariantRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public virtual IList<ShopifyVariant> Retrieve(long shopifyProductId)
        {
            var query = @"SELECT * FROM shopifyvariant WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            return 
                _connection
                    .Query<ShopifyVariant>(query, new { ShopId, shopifyProductId })
                    .ToList();
        }

        public virtual void Insert(ShopifyVariant product)
        {
            product.ShopId = ShopId.Value;
            var query =
                    @"INSERT INTO shopifyvariant(UserId, ShopifyVariantId, ShopifyProductId, Sku, Title, Price) 
                        VALUES(@UserId, @ShopifyVariantId, @ShopifyProductId, @Sku, @Title, @Price)";
            _connection.Execute(query, product);
        }

        public virtual void DeleteByProduct(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyvariant 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { ShopId.Value, shopifyProductId});
        }
    }
}

