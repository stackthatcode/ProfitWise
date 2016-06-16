using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.Factory;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Shopify.Repositories;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Products
{
    public class ProductRefreshService : IDisposable
    {
        private readonly string _userId;
        private readonly ILogger _logger;

        private readonly ProductApiRepository _productApiRepository;
        private readonly ProductDataRepository _productDataRepository;
        private readonly VariantDataRepository _variantDataRepository;
        private readonly MySqlConnection _connection;

        public int ShopifyOrderLimit = 250;
        

        public ProductRefreshService(
                string userId, 
                ILogger logger, 
                ProductApiRepository productApiRepository,
                ProductDataRepository productDataRepository,
                VariantDataRepository variantDataRepository)
        {
            _userId = userId;
            _logger = logger;
            _productApiRepository = productApiRepository;
            _productDataRepository = productDataRepository;
            _variantDataRepository = variantDataRepository;
        }

        public virtual void Execute()
        {
            var allproducts = RetrieveAll();

            WriteAllProductsToDatabase(allproducts);
        }

        public virtual void WriteAllProductsToDatabase(IList<Product> allproducts)
        {
            foreach (var product in allproducts)
            {
                var productData = new ProductData()
                {
                    UserId = _userId,
                    ShopifyProductId = product.Id,
                    Title = product.Title,
                };

                _productDataRepository.Delete(product.Id);
                _variantDataRepository.DeleteByProduct(product.Id);

                _productDataRepository.Insert(productData);

                _logger.Debug(string.Format("{0} - Inserting Product: {1} ({2})", this.ClassAndMethodName(), product.Title, product.Id));

                foreach (var variant in product.Variants)
                {
                    if (variant.Id == 1190225328)
                    {
                        throw new Exception("fuck that shit!!! database no like!!!");
                    }
                    var variantData = new VariantData()
                    {
                        UserId = _userId,
                        ShopifyVariantId = variant.Id,
                        ShopifyProductId = product.Id,
                        Price = variant.Price,
                        Sku = variant.Sku,
                        Title = variant.Title,
                    };

                    _logger.Debug(string.Format("{0} - Inserting Variant: {1} ({2})", this.ClassAndMethodName(), variant.Title, variant.Id));
                    _variantDataRepository.Insert(variantData);
                };
            }
        }

        public virtual IList<Product> RetrieveAll()
        {
            var count = _productApiRepository.RetrieveCount();
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

                var products = _productApiRepository.Retrieve(pagenumber, ShopifyOrderLimit);
                results.AddRange(products);
            }

            TimeSpan ts = stopWatch.Elapsed;
            _logger.Debug(
                string.Format(
                    "{2} total execution time {0} to fetch {1} Products", 
                    ts.ToFormattedString(), results.Count, this.ClassAndMethodName()));

            return results;
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}

