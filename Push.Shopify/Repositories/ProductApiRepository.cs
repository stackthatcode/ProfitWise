using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Components.DictionaryAdapter.Xml;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Interfaces;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{

    [Intercept(typeof(ShopifyCredentialRequired))]
    public class ProductApiRepository : IShopifyCredentialConsumer, IProductApiRepository
    {
        private readonly IHttpClientFacade _client;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly IPushLogger _logger;
        public ShopifyCredentials ShopifyCredentials { get; set; }


        public ProductApiRepository(
                IHttpClientFacade client,
                ShopifyClientConfig configuration,
                ShopifyRequestFactory requestFactory, 
                IPushLogger logger)
        {
            _client = client;
            _client.Configuration = configuration;
            _requestFactory = requestFactory;
            _logger = logger;
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

        public virtual IList<Product> Retrieve(
                ProductFilter filter, int page = 1, int limit = 250, bool includeCost = false)
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
            _logger.Trace(clientResponse.Body);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Product>();

            foreach (var product in parent.products)
            {
                var resultProduct =
                    new Product
                    {
                        Id = product.id,
                        Title = product.title,
                        Tags = product.tags,
                        Vendor = product.vendor,
                        ProductType = product.product_type,
                        Variants = new List<Variant>(),
                    };


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
                            Inventory = variant.inventory_quantity,
                            InventoryManagement = variant.inventory_management,
                            InventoryItemId = variant.inventory_item_id,
                        });
                }

                var ids = resultProduct.Variants.Select(x => x.InventoryItemId).ToList();

                var inventoryItems = RetrieveInventoryItems(ids);

                foreach (var item in inventoryItems.inventory_items)
                {
                    var variant =
                        resultProduct.Variants.FirstOrDefault(x => x.InventoryItemId == item.id);

                    if (variant != null)
                    {
                        variant.Cost = item.cost ?? 0m;
                    }
                }

                results.Add(resultProduct);
            }

            return results;
        }

        public virtual InventoryItemList RetrieveInventoryItems(IList<long> inventoryItemIds)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("ids", inventoryItemIds.ToCommaSeparatedList())
                    .ToString();

            var path = "/admin/inventory_items.json?" + querystring;

            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);

            return JsonConvert.DeserializeObject<InventoryItemList>(clientResponse.Body);
        }
    }
}
