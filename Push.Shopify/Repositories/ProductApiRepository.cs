using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{

    [Intercept(typeof(ShopifyCredentialRequired))]
    public class ProductApiRepository : IShopifyCredentialConsumer
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

        public int RetrieveCount(ProductFilter filter)
        {
            var path = "/admin/api/2019-07/products/count.json?" + filter.ToQueryStringBuilder();
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public ListOfProducts RetrieveByPath(string path)
        {
            var request = _requestFactory.HttpGet(ShopifyCredentials, path, excludeBaseUrl:true);
            var clientResponse = _client.ExecuteRequest(request);

            return ProcessRetrieve(clientResponse);
        }

        public ListOfProducts Retrieve(ProductFilter filter, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var path = "/admin/api/2019-07/products.json?" + querystring;

            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);
            
            return ProcessRetrieve(clientResponse);
        }


        public ListOfProducts ProcessRetrieve(HttpClientResponse clientResponse)
        {
            _logger.Trace(clientResponse.Body);
            var link = clientResponse.Headers.ContainsKey("Link") ? clientResponse.Headers["Link"] : "";

            _logger.Debug($"Status Code: {clientResponse.StatusCode}");
            _logger.Debug($"Response Body: {clientResponse.Body}");
            _logger.Debug($"Link: {link}");

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

            return new ListOfProducts()
            {
                Products = results,
                Link = link,
            };
        }

        public InventoryItemList RetrieveInventoryItems(IList<long> inventoryItemIds)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("ids", inventoryItemIds.ToCommaSeparatedList())
                    .ToString();

            var path = "/admin/api/2019-07/inventory_items.json?" + querystring;

            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);

            return JsonConvert.DeserializeObject<InventoryItemList>(clientResponse.Body);
        }
    }
}
