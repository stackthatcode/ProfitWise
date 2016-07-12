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

        public virtual IList<ShopifyVariant> RetrieveAll()
        {
            var query = @"SELECT * FROM shopifyvariant WHERE ShopId = @ShopId";
            return
                _connection
                    .Query<ShopifyVariant>(query, new { ShopId })
                    .ToList();
        }

        public virtual IList<ShopifyVariant> Retrieve(long shopifyProductId)
        {
            var query = @"SELECT * FROM shopifyvariant WHERE ShopId = @ShopId AND ShopifyProductId = @ShopifyProductId";
            return 
                _connection
                    .Query<ShopifyVariant>(query, new { ShopId, shopifyProductId })
                    .ToList();
        }

        public virtual void Insert(ShopifyVariant product)
        {
            product.ShopId = ShopId.Value;
            var query =
                @"INSERT INTO shopifyvariant( 
                    ShopId, ShopifyVariantId, ShopifyProductId, Sku, Title, Price, Inventory, PwProductId ) 
                VALUES( @ShopId, @ShopifyVariantId, @ShopifyProductId, @Sku, @Title, @Price, @Inventory, @PwProductId )";
            _connection.Execute(query, product);
        }

        public virtual void Update(ShopifyVariant product)
        {
            product.ShopId = ShopId.Value;
            var query =
                @"UPDATE shopifyvariant SET Sku = @Sku, Title = @Title, Price = @Price, Inventory = @Inventory 
                    WHERE ShopId = @ShopId 
                    AND ShopifyVariantId = @ShopifyVariantId
                    AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, product);
        }


        public virtual void DeleteByProduct(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyvariant 
                        WHERE ShopId = @ShopId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { ShopId, shopifyProductId});
        }
    }
}

