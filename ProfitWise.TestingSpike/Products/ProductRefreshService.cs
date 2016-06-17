using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProfitWise.Batch.MultiTenantFactories;
using ProfitWise.Data.Model;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Products
{
    public class ProductRefreshService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly SqlRepositoryFactory _sqlRepositoryFactory;

        public int ShopifyOrderLimit = 250;
        

        public ProductRefreshService(
                ILogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                SqlRepositoryFactory sqlRepositoryFactory)
        {
            _logger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _sqlRepositoryFactory = sqlRepositoryFactory;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            var allproducts = RetrieveAll(shopCredentials);

            WriteAllProductsToDatabase(shopCredentials, allproducts);
        }


        public virtual IList<Product> RetrieveAll(ShopifyCredentials shopCredentials)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);
            
            var count = productApiRepository.RetrieveCount();
            _logger.Info(string.Format("{0} - Executing", this.ClassAndMethodName()));

            var numberofpages = PagingFunctions.NumberOfPages(ShopifyOrderLimit, count);
            var results = new List<Product>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _logger.Debug(
                    string.Format(
                        "{2} - page {0} of {1} pages", pagenumber, numberofpages, this.ClassAndMethodName()));

                var products = productApiRepository.Retrieve(pagenumber, ShopifyOrderLimit);
                results.AddRange(products);
            }

            TimeSpan ts = stopWatch.Elapsed;
            _logger.Debug(
                string.Format(
                    "{2} total execution time {0} to fetch {1} Products",
                    ts.ToFormattedString(), results.Count, this.ClassAndMethodName()));

            return results;
        }

        public virtual void WriteAllProductsToDatabase(ShopifyCredentials shopCredentials, IList<Product> allproducts)
        {
            var productDataRepository = this._sqlRepositoryFactory.MakeProductDataRepository(shopCredentials.ShopOwnerId);
            var variantDataRepository = this._sqlRepositoryFactory.MakeVariantDataRepository(shopCredentials.ShopOwnerId);


            foreach (var product in allproducts)
            {
                var productData = new ProductData()
                {
                    UserId = shopCredentials.ShopOwnerId,
                    ShopifyProductId = product.Id,
                    Title = product.Title,
                };

                productDataRepository.Delete(product.Id);
                variantDataRepository.DeleteByProduct(product.Id);

                productDataRepository.Insert(productData);

                _logger.Debug(string.Format("{0} - Inserting Product: {1} ({2})", this.ClassAndMethodName(), product.Title, product.Id));

                foreach (var variant in product.Variants)
                {
                    if (variant.Id == 1190225328)
                    {
                        throw new Exception("fuck that shit!!! database no like!!!");
                    }
                    var variantData = new VariantData()
                    {
                        UserId = shopCredentials.ShopOwnerId,
                        ShopifyVariantId = variant.Id,
                        ShopifyProductId = product.Id,
                        Price = variant.Price,
                        Sku = variant.Sku,
                        Title = variant.Title,
                    };

                    _logger.Debug(string.Format("{0} - Inserting Variant: {1} ({2})", this.ClassAndMethodName(), variant.Title, variant.Id));
                    variantDataRepository.Insert(variantData);
                };
            }
        }
        
    }
}

