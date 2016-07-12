using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopIdRequired))]
    public class PwProductRepository : IShopIdFilter
    {
        private readonly MySqlConnection _connection;

        public int? ShopId { get; set; }

        public PwProductRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public PwProduct RetrieveAll()
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE ShopId = @ShopId";
            return _connection
                    .Query<PwProduct>(query, new { @ShopId = this.ShopId } ).FirstOrDefault();
        }

        public long Insert(PwProduct product)
        {
            var query = @"INSERT INTO profitwiseproduct ( ShopId, ProductTitle, VariantTitle, Name, Sku ) 
                        VALUES ( @ShopId, @ProductTitle, @VariantTitle, @Name, @Sku );
                        SELECT LAST_INSERT_ID();";
            return _connection.Query<long>(query, product).FirstOrDefault();
        }

        public void Update(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct 
                            SET ProductTitle = @ProductTitle,
                                VariantTitle = @VariantTitle,
                                Name = @Name,
                                Sku = @Sku
                            WHERE ShopId = @ShopId AND PwProductId = @PwProductId";
            _connection.Execute(query, product);
        }


    }
}

