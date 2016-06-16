using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace Push.Shopify.Repositories
{
    public class OrderApiRepository
    {
        private readonly IShopifyHttpClient _client;
        private readonly ILogger _logger;


        public OrderApiRepository(IShopifyHttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }


        public string TempDateFilter = "status=any&created_at_min=2016-01-01T00%3A00%3A00-04%3A00%0A";


        public int RetrieveCount()
        {
            var clientResponse = _client.HttpGet("/admin/orders/count.json" + "?" + TempDateFilter);
            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }



        // TODO => create all the variations of filtering Orders
        public IList<Order> Retrieve(int page = 1, int limit = 250)
        {
            var path = string.Format("/admin/orders.json?page={0}&limit={1}" + "&" + TempDateFilter, page, limit);
            var clientResponse = _client.HttpGet(path);
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
