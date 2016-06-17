using System.Collections.Generic;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    public class ProductDataRepository : IUserIdConsumer
    {
        private readonly MySqlConnection _connection;
        public string UserId { get; set; }


        public ProductDataRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public virtual IList<ProductData> RetrieveAll(string userId)
        {
            var query = @"SELECT * FROM shopifyproduct WHERE UserId";
            return
                _connection
                    .Query<ProductData>(query, new { UserId })
                    .ToList();
        }

        public virtual void Insert(ProductData product)
        {
            product.UserId = UserId;
            var query = @"INSERT INTO shopifyproduct(UserId, ShopifyProductId, Title) 
                        VALUES(@UserId, @ShopifyProductId, @Title)";
            _connection.Execute(query, product);
        }

        public virtual void Delete(long shopifyProductId)
        {
            var query = @"DELETE FROM shopifyproduct 
                        WHERE UserId = @UserId AND ShopifyProductId = @ShopifyProductId";
            _connection.Execute(query, new { UserId, shopifyProductId });
        }

    }
}

