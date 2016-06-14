using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace Push.Shopify.Repositories
{
    public class ProductApiRepository
    {
        private readonly ShopifyHttpClient3 _client;
        private readonly ILogger _logger;

        public ProductApiRepository(ShopifyHttpClient3 client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public int RetrieveCount()
        {
            var json = _client.HttpGet("/admin/products/count.json");
            dynamic parent = JsonConvert.DeserializeObject(json.ResponseBody);
            var count = parent.count;
            return count;
        }

        public IList<Product> Retrieve(int page = 1, int limit = 250)
        {
            var path = string.Format("/admin/products.json?page={0}&limit={1}", page, limit);
            var json = _client.HttpGet(path);
            dynamic parent = JsonConvert.DeserializeObject(json.ResponseBody);

            var results = new List<Product>();

            foreach (var product in parent.products)
            {
                var resultProduct = 
                    new Product()
                    {
                        Id = product.id,
                        Title = product.title,
                    };
                resultProduct.Variants = new List<Variant>();

                foreach (var variant in product.variants)
                {
                    resultProduct.Variants.Add(
                        new Variant()
                        {
                            Id = variant.id,
                            Title = variant.title,
                            Price = variant.price,
                            Sku = variant.sku,
                            ParentProduct = resultProduct,
                        });
                }

                results.Add(resultProduct);
            }

            return results;
        }

        public IList<Product> RetrieveAll(int limit = 250, int delay = 500)
        {
            var count = RetrieveCount();
            var numberofpages = PagingFunctions.NumberOfPages(limit, count);
            var results = new List<Product>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _logger.Debug(
                    string.Format(
                        "ProductRepository->RetrieveAll() - page {0} of {1} pages", pagenumber, numberofpages));

                var products = Retrieve(pagenumber, limit);
                results.AddRange(products);
                Thread.Sleep(delay);
            }

            return results;
        }
    }
}
