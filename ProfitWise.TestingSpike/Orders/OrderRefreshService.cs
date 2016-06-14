using Push.Shopify.HttpClient;
using Push.Shopify.Repositories;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Orders
{
    public class OrderRefreshService
    {
        private readonly OrderRepository _orderRepository;
        private readonly ILogger _logger;

        public OrderRefreshService(ShopifyHttpClient3 shopifyClient, ILogger logger)
        {
            _orderRepository = new OrderRepository(shopifyClient, logger);
            _logger = logger;
        }

        public void Execute()
        {
            var results = _orderRepository.Retrieve();
            foreach (var order in results)
            {
                var message = string.Format("Order found: {0} - {1}", order.Id, order.Email);
                _logger.Info(message);
            }
        }
    }
}

