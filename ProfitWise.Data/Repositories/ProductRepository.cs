using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Dapper;
using MySql.Data.MySqlClient;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories
{
    [Intercept(typeof(ShopRequired))]
    public class ProductRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;

        private readonly ConnectionWrapper _connectionWrapper;
        private IDbConnection Connection => _connectionWrapper.DbConn;

        public ProductRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();
        }


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

        public long RetrieveMasterProductId(long pwProductId)
        {
            var query = @"SELECT PwMasterProductId FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId AND PwProductId = @pwProductId";

            return Connection.Query<long>(
                query, new { PwShop.PwShopId, pwProductId }, _connectionWrapper.Transaction).First();
        }

        public PwMasterProduct RetrieveMasterProduct(long pwMasterProductId)
        {
            var products = RetrieveProducts(pwMasterProductId);
            var masterProduct = new PwMasterProduct()
            {
                PwMasterProductId = pwMasterProductId,
                PwShopId = this.PwShopId,
                Products = products,
            };

            foreach (var product in products)
            {
                product.ParentMasterProduct = masterProduct;
            }
        
            return masterProduct;
        }

        public long InsertMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                    @"INSERT INTO profitwisemasterproduct ( PwShopId )
                    VALUES ( @PwShopId );
                    SELECT SCOPE_IDENTITY();";

            return Connection
                .Query<long>(query, masterProduct, _connectionWrapper.Transaction).FirstOrDefault();
        }

        public void DeleteMasterProduct(PwMasterProduct masterProduct)
        {
            var query =
                @"DELETE FROM profitwisemasterproduct " +
                @"WHERE PwShopId = @PwShopId AND PwMasterProductId = @PwMasterProductId;";

            Connection.Execute(query, masterProduct, _connectionWrapper.Transaction);
        }

        public IList<PwProduct> RetrieveAllProducts()
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId";
            return Connection.Query<PwProduct>(
                query, new { @PwShopId = this.PwShop.PwShopId }, _connectionWrapper.Transaction).ToList();
        }

        public IList<PwProduct> RetrieveProducts(long pwMasterProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct WHERE PwShopId = @PwShopId AND PwMasterProductId = @pwMasterProductId";
            return Connection.Query<PwProduct>(
                query, new { @PwShopId = this.PwShop.PwShopId, pwMasterProductId }, 
                _connectionWrapper.Transaction).ToList();
        }


        public PwProduct RetrieveProduct(long pwProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return Connection
                    .Query<PwProduct>(
                                query, new { @PwShopId = this.PwShop.PwShopId, @PwProductId = pwProductId },
                                _connectionWrapper.Transaction)
                    .FirstOrDefault();
        }

        public PwProduct RetrievePrimaryProduct(long pwMasterProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct 
                        WHERE PwShopId = @PwShopId 
                        AND PwMasterProductId = @pwMasterProductId
                        AND IsPrimary = 1;";

            return Connection
                    .Query<PwProduct>(
                                query, new { PwShop.PwShopId, pwMasterProductId },
                                _connectionWrapper.Transaction)
                    .FirstOrDefault();
        }

        public IList<long> RetrieveAllChildProductIds(long pwMasterProductId)
        {
            var query = @"SELECT * FROM profitwiseproduct
                        WHERE PwShopId = @PwShopId AND PwMasterProductId = @pwMasterProductId";
            return Connection.Query<long>(
                query, new { @PwShopId = this.PwShop.PwShopId, pwMasterProductId },
                _connectionWrapper.Transaction).ToList();
        }

        public long InsertProduct(PwProduct product)
        {
            var query = @"INSERT INTO profitwiseproduct 
                            ( PwShopId, PwMasterProductId, ShopifyProductId, Title, Vendor, ProductType, Tags,
                            IsActive, IsPrimary, IsPrimaryManual, LastUpdated ) 
                        VALUES ( @PwShopId, @PwMasterProductId, @ShopifyProductId, @Title, @Vendor, @ProductType, @Tags,
                            @IsActive, @IsPrimary, @IsPrimaryManual, @LastUpdated );
                        SELECT SCOPE_IDENTITY();";
            return Connection.Query<long>(query, product, _connectionWrapper.Transaction).FirstOrDefault();
        }

        public void UpdateProduct(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET Tags = @Tags,
                                ProductType = @ProductType,
                                LastUpdated = @LastUpdated
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            Connection.Execute(query, product, _connectionWrapper.Transaction);
        }

        public void UpdateProductsMasterProduct(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET PwMasterProductId = @PwMasterProductId                            
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";

            Connection.Execute(query, product, _connectionWrapper.Transaction);
        }

        public void UpdateProductIsActive(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                            SET IsActive = @IsActive
                            WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";
            Connection.Execute(query, product, _connectionWrapper.Transaction);
        }

        public void UpdateProductIsPrimary(PwProduct product)
        {
            var query = @"UPDATE profitwiseproduct
                        SET IsPrimary = @IsPrimary, IsPrimaryManual = @IsPrimaryManual
                        WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId";

            Connection.Execute(query, product, _connectionWrapper.Transaction);
        }


        public void UpdateProductIsActiveByShopifyId(long shopifyProductId, bool isActive)
        {
            var query = @"UPDATE profitwiseproduct SET IsActive = @IsActive
                            WHERE PwShopId = @PwShopId 
                            AND ShopifyProductId = @shopifyProductId";
            Connection.Execute(query, 
                    new
                    {
                        @PwShopId = this.PwShop.PwShopId,
                        ShopifyProductId = shopifyProductId,
                        IsActive = isActive,
                    }, _connectionWrapper.Transaction);
        }

        public PwProduct DeleteProduct(long pwProductId)
        {
            var query = @"DELETE FROM profitwiseproduct WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return Connection
                    .Query<PwProduct>(
                        query, new { @PwShopId = this.PwShop.PwShopId, @PwProductId = pwProductId }, 
                        _connectionWrapper.Transaction)
                    .FirstOrDefault();
        }

        public void DeleteOrphanedMasterProducts()
        {
            var query = @"DELETE FROM profitwisemasterproduct
                        WHERE PwShopId = @PwShopId AND PwMasterProductId NOT IN 
                            (SELECT PwMasterProductId FROM profitwiseproduct);";
            Connection.Execute(query, new {@PwShopId = this.PwShop.PwShopId,}, 
                                _connectionWrapper.Transaction);
        }
    }
}

