using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    public class OrderApiRepository : IShopifyCredentialConsumer
    {
        private readonly IShopifyHttpClient _client;
        private readonly ShopifyRequestFactory _requestFactory;

        public ShopifyCredentials ShopifyCredentials { get; set; }


        public OrderApiRepository(
                IShopifyHttpClient client, 
                ShopifyRequestFactory requestFactory)
        {
            _client = client;
            _requestFactory = requestFactory;
        }

        public string TempDateFilter = "status=any&created_at_min=2016-01-01T00%3A00%3A00-04%3A00%0A";

        public virtual int RetrieveCount()
        {
            var request = _requestFactory.HttpGet(ShopifyCredentials, "/admin/orders/count.json" + "?" + TempDateFilter);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Order> Retrieve(int page = 1, int limit = 250)
        {
            var path = string.Format("/admin/orders.json?page={0}&limit={1}" + "&" + TempDateFilter, page, limit);
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Order>();

            foreach (var order in parent.orders)
            {
                results.Add(new Order
                {
                    Id = order.id,
                    Email = order.email,
                }); 
            }

            return results;
        }

    }
}
