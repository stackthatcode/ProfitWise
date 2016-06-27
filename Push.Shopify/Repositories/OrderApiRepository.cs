using System;
using System.Collections.Generic;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Aspect;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Repositories
{
    [Intercept(typeof(ShopifyCredentialRequired))]
    public class OrderApiRepository : IShopifyCredentialConsumer
    {
        private readonly IShopifyHttpClient _client;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly IPushLogger _logger;

        public ShopifyCredentials ShopifyCredentials { get; set; }

        public OrderApiRepository(
                IShopifyHttpClient client, 
                ShopifyRequestFactory requestFactory,
                IPushLogger logger)
        {
            _client = client;
            _requestFactory = requestFactory;
            _logger = logger;
        }


        public virtual int RetrieveCount(OrderFilter filter)
        {
            var url = "/admin/orders/count.json?" + filter.ToQueryStringBuilder();
            var request = _requestFactory.HttpGet(ShopifyCredentials, url);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual IList<Order> Retrieve(OrderFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();


            var path = string.Format("/admin/orders.json?" + querystring);
            var request = _requestFactory.HttpGet(ShopifyCredentials, path);
            var clientResponse = _client.ExecuteRequest(request);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Order>();

            foreach (var order in parent.orders)
            {
                if (_logger.IsTraceEnabled)
                {
                    _logger.Trace($"Dump of Order ID: {order.id}" +
                                    Environment.NewLine +
                                    JsonConvert.SerializeObject(order));
                }

                _logger.Debug($"Deserializing Order {order.name} ({order.id})");

                var orderResult = new Order
                {
                    Id = order.id,
                    Email = order.email,
                    Name = order.name,
                    TotalPrice = order.total_price,
                    LineItems = new List<OrderLineItem>()
                };

                foreach (var line_item in order.line_items)
                {
                    _logger.Debug($"Deserializing Order Line Item {line_item.id}");

                    var orderLineItemResult = new OrderLineItem();

                    orderLineItemResult.Id = line_item.id;
                    orderLineItemResult.Discount = line_item.total_discount;
                    orderLineItemResult.ProductId = line_item.product_id;
                    orderLineItemResult.VariantId = line_item.variant_id;
                    orderLineItemResult.Price = line_item.price;
                    orderLineItemResult.Quantity = line_item.quantity;
                    orderLineItemResult.Sku = line_item.sku;

                    // Taxes = line_item. TODO *** pull in all the tax_lines...?

                    orderResult.LineItems.Add(orderLineItemResult);
                }
                results.Add(orderResult); 
            }

            return results;
        }
    }
}
