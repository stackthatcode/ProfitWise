using System;
using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]

    public class ProductApiRepository : IShopifyCredentialConsumer
    {
        private readonly IShopifyHttpClient _client;
        private readonly ShopifyRequestFactory _requestFactory;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public ProductApiRepository(
                IShopifyHttpClient client, 
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _requestFactory = requestFactory;
        }

        public virtual int RetrieveCount(ProductFilter filter)
        {
            var path = "/admin/products/count.json?" + filter.ToQueryStringBuilder();

            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Product> Retrieve(ProductFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var path = "/admin/products.json?" + querystring;

            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);

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
                            UpdatedAt = variant.updated_at,
                            Inventory = variant.inventory_quantity
                        });
                }

                results.Add(resultProduct);
            }

            return results;
        }
    }
}
