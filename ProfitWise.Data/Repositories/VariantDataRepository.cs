using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    

    public class VariantDataRepository
    {
        private readonly string _userId;
        private readonly MySqlConnection _connection;

        public VariantDataRepository(string userId, MySqlConnection connection)
        {
            _userId = userId;
            _connection = connection;
        }

        public IList<VariantData> Retrieve(long shopifyProductId)
        {
            var query = @"SELECT * FROM shopifyvariant WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            return 
                _connection
                    .Query<VariantData>(query, new { UserId = _userId, shopifyProductId })
                    .ToList();
        }

        public void Insert(VariantData product)
        {
            product.UserId = _userId;
            var query =
                    @"INSERT INTO shopifyvariant(UserId, ShopifyVariantId, ShopifyProductId, Sku, Title, Price) 
                        VALUES(@UserId, @ShopifyVariantId, @ShopifyProductId, @Sku, @Title, @Price)";
            _connection.Execute(query, product);
        }

        public void DeleteByProduct(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyvariant 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { UserId = _userId, shopifyProductId});
        }
    }
}

