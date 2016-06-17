using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    

    public class VariantDataRepository : IUserIdConsumer
    {
        private readonly MySqlConnection _connection;
        public string UserId { get; set; }

        public VariantDataRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public virtual IList<VariantData> Retrieve(long shopifyProductId)
        {
            var query = @"SELECT * FROM shopifyvariant WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            return 
                _connection
                    .Query<VariantData>(query, new { UserId, shopifyProductId })
                    .ToList();
        }

        public virtual void Insert(VariantData product)
        {
            product.UserId = UserId;
            var query =
                    @"INSERT INTO shopifyvariant(UserId, ShopifyVariantId, ShopifyProductId, Sku, Title, Price) 
                        VALUES(@UserId, @ShopifyVariantId, @ShopifyProductId, @Sku, @Title, @Price)";
            _connection.Execute(query, product);
        }

        public virtual void DeleteByProduct(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyvariant 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { UserId, shopifyProductId});
        }
    }
}

