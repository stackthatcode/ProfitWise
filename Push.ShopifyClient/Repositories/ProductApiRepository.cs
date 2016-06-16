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
        private readonly IShopifyHttpClient _client;
        private readonly ILogger _logger;

        public ProductApiRepository(IShopifyHttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public int RetrieveCount()
        {
            var json = _client.HttpGet("/admin/products/count.json");
            dynamic parent = JsonConvert.DeserializeObject(json.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Product> Retrieve(int page = 1, int limit = 250)
        {
            var path = string.Format("/admin/products.json?page={0}&limit={1}", page, limit);
            var json = _client.HttpGet(path);
            dynamic parent = JsonConvert.DeserializeObject(json.Body);

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
    }
}
