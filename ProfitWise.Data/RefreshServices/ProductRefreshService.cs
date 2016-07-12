using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.RefreshServices
{
    public class ProductRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly ShopRepository _shopRepository;


        public ProductRefreshService(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration configuration,
                ShopRepository shopRepository)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantRepositoryFactory = multitenantRepositoryFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Load Batch State
            var batchStateRepository = _multitenantRepositoryFactory.MakeBatchStateRepository(shop);
            var batchState = batchStateRepository.Retrieve();

            // Import Products from Shopify
            var importedProducts = RetrieveAll(shopCredentials, batchState);
            
            // Write Products to our database
            WriteAllProductsToDatabase(shop, importedProducts);

            // Update Batch State
            batchState.ProductsLastUpdated = DateTime.Now.AddMinutes(-15);
            batchStateRepository.Update(batchState);
        }



        public virtual IList<Product> RetrieveAll(ShopifyCredentials shopCredentials, PwBatchState batchState)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);
            var filter = new ProductFilter()
            {
                UpdatedAtMin = batchState.ProductsLastUpdated,
            };
            var count = productApiRepository.RetrieveCount(filter);


            _pushLogger.Info($"{this.ClassAndMethodName()} - Executing");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Product>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");

                var products = productApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(products);
            }

            return results;
        }

        public virtual void WriteAllProductsToDatabase(ShopifyShop shop, IList<Product> new_products)
        {     
            _pushLogger.Info($"{this.ClassAndMethodName()} - {new_products.Count} Products to process");

            var productDataRepository = this._multitenantRepositoryFactory.MakeShopifyProductRepository(shop);
            var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);
            var profitWiseProductRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);

            var existingProducts = productDataRepository.RetrieveAll();
            var existingVariants = variantDataRepository.RetrieveAll();
            var profitWiseProducts = profitWiseProductRepository.RetrieveAll();


            foreach (var product in new_products)
            {
                WriteProductToDatabase(shop, product, existingProducts, productDataRepository);

                foreach (var variant in product.Variants)
                {
                    WriteVariantToDatabase(
                            shop, variant, existingVariants, profitWiseProducts,
                            variantDataRepository, profitWiseProductRepository);
                }
            }
        }


        private void WriteProductToDatabase(
                ShopifyShop shop, Product importedProduct, IList<ShopifyProduct> existingProducts,
                ShopifyProductRepository productDataRepository)
        {
            var existingProduct = existingProducts.FirstOrDefault(x => x.ShopifyProductId == importedProduct.Id);

            if (existingProduct == null)
            {
                var productData = new ShopifyProduct()
                {
                    ShopId = shop.ShopId,
                    ShopifyProductId = importedProduct.Id,
                    Title = importedProduct.Title,
                };

                _pushLogger.Debug(
                    $"{this.ClassAndMethodName()} - Inserting Product: {importedProduct.Title} ({importedProduct.Id})");
                productDataRepository.Insert(productData);
            }
            else
            {
                existingProduct.Title = importedProduct.Title;
                _pushLogger.Debug(
                    $"{this.ClassAndMethodName()} - Updating Product: {importedProduct.Title} ({importedProduct.Id})");

                productDataRepository.Update(existingProduct);
            }
        }


        private void WriteVariantToDatabase(
                    ShopifyShop shop, Variant importedVariant, 
                    IList<ShopifyVariant> existingVariants, PwProduct profitWiseProducts, 
                    ShopifyVariantRepository variantDataRepository, PwProductRepository profitWiseProductRepository)
        {
            var existingVariant = 
                existingVariants.FirstOrDefault(x => 
                        x.ShopifyProductId == importedVariant.ParentProduct.Id && 
                        x.ShopifyVariantId == importedVariant.Id);

            if (existingVariant == null)
            {
                _pushLogger.Debug($"{this.ClassAndMethodName()} - Inserting Variant: {importedVariant.Title} ({importedVariant.Id})");

                var profitWiseProduct = new PwProduct
                {
                    ShopId = shop.ShopId,
                    ProductTitle = importedVariant.ParentProduct.Title,
                    VariantTitle = importedVariant.Title,
                    Sku = importedVariant.Sku,
                    Name = importedVariant.ParentProduct.Title + " - " + importedVariant.Title,
                };

                var newPwProductId = profitWiseProductRepository.Insert(profitWiseProduct);

                var variantData = new ShopifyVariant()
                {
                    ShopId = shop.ShopId,
                    ShopifyVariantId = importedVariant.Id,
                    ShopifyProductId = importedVariant.ParentProduct.Id,
                    Price = importedVariant.Price,
                    Sku = importedVariant.Sku,
                    Title = importedVariant.Title,
                    Inventory = importedVariant.Inventory,
                    PwProductId = newPwProductId,
                };

                variantDataRepository.Insert(variantData);
            }
            else
            {
                existingVariant.Sku = importedVariant.Sku;
                existingVariant.Title = importedVariant.Title;
                existingVariant.Price = importedVariant.Price;
                existingVariant.Inventory = importedVariant.Inventory;

                _pushLogger.Debug($"{this.ClassAndMethodName()} - Updating Variant: {importedVariant.Title} ({importedVariant.Id})");
                variantDataRepository.Update(existingVariant);
            }
        }
    }
}

