using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    public class ProductDataRepository
    {
        private readonly string _userId;
        private readonly MySqlConnection _connection;

        public ProductDataRepository(string userId, MySqlConnection connection)
        {
            _userId = userId;
            _connection = connection;
        }

        public IList<ProductData> RetrieveAll()
        {
            var query = @"SELECT * FROM shopifyproduct WHERE UserId";
            return
                _connection
                    .Query<ProductData>(query, new { UserId = _userId })
                    .ToList();
        }

        public void Insert(ProductData product)
        {
            product.UserId = _userId;
            var query = @"INSERT INTO shopifyproduct(UserId, ShopifyProductId, Title) 
                        VALUES(@UserId, @ShopifyProductId, @Title)";
            _connection.Execute(query, product);
        }

        public void Delete(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyproduct 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { UserId = _userId, shopifyProductId });
        }

    }
}

