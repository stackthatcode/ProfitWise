using System;
using System.Collections.Generic;
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
            var batchStateRepository = _multitenantRepositoryFactory.MakeProfitWiseBatchStateRepository(shop);
            var batchState = batchStateRepository.Retrieve();

            // Retreive Products from Shopify
            var allproducts = RetrieveAll(shopCredentials, batchState);
            
            // Write Products to our database
            WriteAllProductsToDatabase(shop, allproducts);

            // Update Batch State
            batchState.ProductsLastUpdated = DateTime.Now;
            batchStateRepository.Update(batchState);
        }



        public virtual IList<Product> RetrieveAll(ShopifyCredentials shopCredentials, ProfitWiseBatchState batchState)
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

        public virtual void WriteAllProductsToDatabase(ShopifyShop shop, IList<Product> allproducts)
        {     
            _pushLogger.Info($"{this.ClassAndMethodName()} - {allproducts.Count} Products to process");

            var productDataRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);
            var variantDataRepository = this._multitenantRepositoryFactory.MakeVariantRepository(shop);

            foreach (var product in allproducts)
            {
                var productData = new ShopifyProduct()
                {
                    ShopId = shop.ShopId,
                    ShopifyProductId = product.Id,
                    Title = product.Title,
                };

                productDataRepository.Delete(product.Id);
                variantDataRepository.DeleteByProduct(product.Id);

                productDataRepository.Insert(productData);

                _pushLogger.Debug($"{this.ClassAndMethodName()} - Inserting Product: {product.Title} ({product.Id})");

                foreach (var variant in product.Variants)
                {                    
                    var variantData = new ShopifyVariant()
                    {
                        ShopId = shop.ShopId,
                        ShopifyVariantId = variant.Id,
                        ShopifyProductId = product.Id,
                        Price = variant.Price,
                        Sku = variant.Sku,
                        Title = variant.Title,
                    };

                    _pushLogger.Debug($"{this.ClassAndMethodName()} - Inserting Variant: {variant.Title} ({variant.Id})");
                    variantDataRepository.Insert(variantData);
                };
            }
        }
    }
}

