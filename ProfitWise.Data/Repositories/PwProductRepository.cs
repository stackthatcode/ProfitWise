using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Model;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class PwProductRepository : IShopFilter
    {
        private readonly MySqlConnection _connection;
        public PwShop PwShop { get; set; }

        public PwProductRepository(MySqlConnection connection)
        {
            _connection = connection;
        }


        //
        // TODO => add paging and filtering
        //
        public IList<PwMasterProduct> RetrieveAllMasterProducts()
        {
            var products = RetrieveAllProducts();

            var masterProducts = 
                products
                    .Distinct()    
                    .Select(masterProduct => new PwMasterProduct
                    {
                        PwShopId = this.PwShop.PwShopId,
                        PwMasterProductId = masterProduct.PwMasterProductId,
                        Products = products.Where(product => masterProduct.PwMasterProductId == product.PwMasterProductId).ToList()
                    }).ToList();

            foreach (var masterProduct in masterProducts)
            {
                foreach (var product in masterProduct.Products)
                {
                    product.ParentMasterProduct = masterProduct;
                }
            }

            return masterProducts;
        }

        public long InsertMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                    @"INSERT INTO profitwisemasterproduct ( PwShopId )
                    VALUES ( @PwShopId );
                    SELECT LAST_INSERT_ID();";

            return _connection.Query<long>(query, masterProduct).FirstOrDefault();
        }

        public void DeleteMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                @"DELETE FROM profitwisemasterproduct " +
                @"WHERE PwShopId = @PwShopId AND PwMasterProductId = @PwMasterProductId;";

            _connection.Execute(query);
        }



        public IList<PwProduct> RetrieveAllProducts()
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId";
            return _connection
                    .Query<PwProduct>(query, new { @PwShopId = this.PwShop.PwShopId } ).ToList();
        }

        public PwProduct RetrieveProduct(long pwProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return _connection
                    .Query<PwProduct>(query, new { @PwShopId = this.PwShop.PwShopId, @PwProductId = pwProductId })
                    .FirstOrDefault();
        }

        public long InsertProduct(PwProduct product)
        {
            var query = @"INSERT INTO profitwiseproduct 
                            ( PwShopId, PwMasterProductId, ShopifyProductId, Title, Vendor, ProductType, Tags,
                            IsActive, IsPrimary, IsPrimaryManual, LastUpdated ) 
                        VALUES ( @PwShopId, @PwMasterProductId, @ShopifyProductId, @Title, @Vendor, @ProductType, @Tags,
                            @IsActive, @IsPrimary, @IsPrimaryManual, @LastUpdated );
                        SELECT LAST_INSERT_ID();";
            return _connection.Query<long>(query, product).FirstOrDefault();
        }

        public void UpdateProduct(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET Tags = @Tags,
                                ProductType = @ProductType,
                                LastUpdated = @LastUpdated
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            _connection.Execute(query, product);
        }

        public void UpdateProductIsActive(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET IsActive = @IsActive
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            _connection.Execute(query, product);
        }

        public void UpdateProductIsPrimary(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET IsPrimary = @IsPrimary
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            _connection.Execute(query, product);
        }


        public void UpdateProductIsActiveByShopifyId(long shopifyProductId, bool isActive)
        {
            var query = @"UPDATE profitwiseproduct SET IsActive = @IsActive
                            WHERE PwShopId = @PwShopId 
                            AND ShopifyProductId = @shopifyProductId";
            _connection.Execute(query, 
                    new
                    {
                        @PwShopId = this.PwShop.PwShopId,
                        ShopifyProductId = shopifyProductId,
                        IsActive = isActive,
                    });
        }

        public PwProduct DeleteProduct(long pwProductId)
        {
            var query = @"DELETE FROM profitwiseproduct WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return _connection
                    .Query<PwProduct>(
                        query, new { @PwShopId = this.PwShop.PwShopId, @PwProductId = pwProductId })
                    .FirstOrDefault();
        }

        public void DeleteOrphanedMasterProducts()
        {
            var query = @"DELETE FROM profitwisemasterproduct
                        WHERE PwShopId = @PwShopId AND PwMasterProductId NOT IN 
                            (SELECT PwMasterProductId FROM profitwiseproduct);";
            _connection.Execute(query, new {@PwShopId = this.PwShop.PwShopId,});
        }
    }
}

