using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Logging;

namespace Push.Shopify.Repositories
{
    public class OrderRepository
    {
        private readonly ShopifyHttpClient3 _client;
        private readonly ILogger _logger;

        public OrderRepository(ShopifyHttpClient3 client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }


        // TODO => create all the variations of filtering Orders
        public IList<Order> Retrieve()
        {
            var json = _client.HttpGet("/admin/orders.json?limit=10");
            dynamic parent = JsonConvert.DeserializeObject(json.ResponseBody);

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
