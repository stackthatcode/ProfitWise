using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Data.RefreshServices
{
    public class ProductRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantSqlRepositoryFactory _multitenantSqlRepositoryFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly ShopRepository _shopRepository;

        

        public ProductRefreshService(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantSqlRepositoryFactory multitenantSqlRepositoryFactory,
                RefreshServiceConfiguration configuration,
                ShopRepository shopRepository)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantSqlRepositoryFactory = multitenantSqlRepositoryFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }

        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            var allproducts = RetrieveAll(shopCredentials);
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            WriteAllProductsToDatabase(shop, allproducts);
        }

        public virtual IList<Product> RetrieveAll(ShopifyCredentials shopCredentials)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);
            
            var count = productApiRepository.RetrieveCount();
            _pushLogger.Info($"{this.ClassAndMethodName()} - Executing");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProduceRate, count);
            var results = new List<Product>();

            // SEE THIS JEREMY??? NO MORE!!!
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");

                var products = productApiRepository.Retrieve(pagenumber, _configuration.MaxProduceRate);
                results.AddRange(products);
            }

            // SEE THIS JEREMY??? NO MORE!!!
            //TimeSpan ts = stopWatch.Elapsed;
            //_pushLogger.Info(
            //    $"{this.ClassAndMethodName()} total execution time {ts.ToFormattedString()} to fetch {results.Count} Products");

            return results;
        }

        public virtual void WriteAllProductsToDatabase(ShopifyShop shop, IList<Product> allproducts)
        {       
            // SEE THIS JEREMY??? NO MORE!!!
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            
            _pushLogger.Info($"{this.ClassAndMethodName()} - {allproducts.Count} Products to process");

            var productDataRepository = this._multitenantSqlRepositoryFactory.MakeProductRepository(shop);
            var variantDataRepository = this._multitenantSqlRepositoryFactory.MakeVariantRepository(shop);

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

            // SEE THIS JEREMY??? NO MORE!!!
            //TimeSpan ts = stopWatch.Elapsed;
            //_pushLogger.Info($"{this.ClassAndMethodName()} total execution time {ts.ToFormattedString()} to write {allproducts.Count} Products");
        }
    }
}

