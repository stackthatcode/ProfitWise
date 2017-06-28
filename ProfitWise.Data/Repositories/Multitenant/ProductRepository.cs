using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Repositories.Multitenant
{
    [Intercept(typeof(ShopRequired))]
    public class ProductRepository : IShopFilter
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;

        public ProductRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();
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

        public long RetrieveMasterProductByProductId(long pwProductId)
        {
            var query = @"SELECT PwMasterProductId FROM product(@PwShopId) 
                        WHERE PwProductId = @pwProductId";
            return _connectionWrapper.Query<long>(query, new { PwShop.PwShopId, pwProductId }).First();
        }

        public long RetrieveMasterProductByMasterVariantId(long pwMasterVariantId)
        {
            var query = @"SELECT PwMasterProductId FROM mastervariant(@PwShopId) 
                        WHERE PwMasterVariantId = @pwMasterVariantId";

            return _connectionWrapper.Query<long>(query, new { PwShop.PwShopId, pwMasterVariantId }).First();
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
                    @"INSERT INTO masterproduct(@PwShopId) ( PwShopId ) VALUES ( @PwShopId );
                    SELECT SCOPE_IDENTITY();";

            return _connectionWrapper.Query<long>(query, masterProduct).FirstOrDefault();
        }

        public void DeleteMasterProduct(PwMasterProduct masterProduct)
        {
            var query = @"DELETE FROM masterproduct(@PwShopId) 
                        WHERE PwMasterProductId = @PwMasterProductId;";
            _connectionWrapper.Execute(query, masterProduct);
        }

        public IList<PwProduct> RetrieveAllProducts()
        {
            var query = @"SELECT * FROM product(@PwShopId);";
            return _connectionWrapper.Query<PwProduct>(query, new { PwShop.PwShopId }).ToList();
        }

        public IList<PwProduct> RetrieveProducts(long pwMasterProductId)
        {
            var query = @"SELECT * FROM product(@PwShopId) 
                        WHERE PwMasterProductId = @pwMasterProductId";
            return _connectionWrapper.Query<PwProduct>(query, new { PwShopId, pwMasterProductId }).ToList();
        }


        public PwProduct RetrieveProduct(long pwProductId)
        {
            var query = @"SELECT * FROM product(@PwShopId) WHERE PwProductId = @PwProductId;";
            return _connectionWrapper
                    .Query<PwProduct>(query, new { PwShop.PwShopId, @PwProductId = pwProductId })
                    .FirstOrDefault();
        }

        public PwProduct RetrievePrimaryProduct(long pwMasterProductId)
        {
            var query = @"SELECT * FROM product(@PwShopId) 
                        WHERE PwMasterProductId = @pwMasterProductId
                        AND IsPrimary = 1;";

            return _connectionWrapper
                    .Query<PwProduct>(query, new { PwShop.PwShopId, pwMasterProductId })
                    .FirstOrDefault();
        }

        public IList<long> RetrieveAllChildProductIds(long pwMasterProductId)
        {
            var query = @"SELECT * FROM product(@PwShopId) WHERE PwMasterProductId = @pwMasterProductId";
            return _connectionWrapper.Query<long>(query, new { PwShop.PwShopId, pwMasterProductId }).ToList();
        }

        public long InsertProduct(PwProduct product)
        {
            var query = @"INSERT INTO product(@PwShopId) 
                            ( PwShopId, PwMasterProductId, ShopifyProductId, Title, Vendor, ProductType, Tags,
                            IsActive, IsPrimary, IsPrimaryManual, LastUpdated ) 
                        VALUES ( @PwShopId, @PwMasterProductId, @ShopifyProductId, @Title, @Vendor, @ProductType, @Tags,
                            @IsActive, @IsPrimary, @IsPrimaryManual, @LastUpdated );
                        SELECT SCOPE_IDENTITY();";
            return _connectionWrapper.Query<long>(query, product).FirstOrDefault();
        }

        public void UpdateProduct(PwProduct product)
        {
            var query = @"UPDATE product(@PwShopId)
                        SET Tags = @Tags,
                            ProductType = @ProductType,
                            LastUpdated = @LastUpdated
                        WHERE PwProductId = @PwProductId";
            _connectionWrapper.Execute(query, product);
        }

        public void UpdateProductsMasterProduct(PwProduct product)
        {
            var query = @"UPDATE product(@PwShopId) SET PwMasterProductId = @PwMasterProductId                            
                        WHERE PwProductId = @PwProductId";

            _connectionWrapper.Execute(query, product);
        }

        public void UpdateProductIsActive(PwProduct product)
        {
            var query = @"UPDATE product(@PwShopId) SET IsActive = @IsActive WHERE PwProductId = @PwProductId";
            _connectionWrapper.Execute(query, product);
        }

        public void UpdateProductIsPrimary(PwProduct product)
        {
            var query = @"UPDATE product(@PwShopId)
                        SET IsPrimary = @IsPrimary, IsPrimaryManual = @IsPrimaryManual
                        WHERE PwProductId = @PwProductId";

            _connectionWrapper.Execute(query, product);
        }


        public void UpdateProductIsActiveByShopifyId(long shopifyProductId, bool isActive)
        {
            var query = @"UPDATE product(@PwShopId) SET IsActive = @IsActive
                            WHERE ShopifyProductId = @shopifyProductId";
            _connectionWrapper.Execute(query, 
                    new
                    {
                        PwShop.PwShopId,
                        ShopifyProductId = shopifyProductId,
                        IsActive = isActive,
                    });
        }

        public PwProduct DeleteProduct(long pwProductId)
        {
            var query = @"DELETE FROM product(@PwShopId) 
                        WHERE PwShopId = @PwShopId AND PwProductId = @PwProductId;";
            return _connectionWrapper
                    .Query<PwProduct>(query, new { PwShop.PwShopId, @PwProductId = pwProductId })
                    .FirstOrDefault();
        }

        public void DeleteChildlessMasterProducts()
        {
            var query = @"DELETE FROM masterproduct(@PwShopId)
                        WHERE PwMasterProductId NOT IN (SELECT PwMasterProductId FROM product(@PwShopId));";
            _connectionWrapper.Execute(query, new { PwShop.PwShopId,});
        }
    }
}

