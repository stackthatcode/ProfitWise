using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Shopify.Repositories;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Orders
{
    public class OrderRefreshService
    {
        private readonly OrderApiRepository _orderRepository;
        private readonly ILogger _logger;
        private readonly ShopifyRequestFactory _requestFactory;
        private readonly MySqlConnection _connection;

        public int ShopifyOrderLimit = 250;
        

        public OrderRefreshService(
                        ILogger logger, 
                        OrderApiRepository orderApiRepository,
                        MySqlConnectionFactory connectionFactory)
        {
            _logger = logger;
            _orderRepository = orderApiRepository;
            _connection = connectionFactory.Make();
        }

        public virtual void Execute(string userId)
        {
            var results = RetrieveAll();

            foreach (var order in results)
            {
                var message = string.Format("Order found: {0} - {1}", order.Id, order.Email);
                _logger.Debug(message);


            }
        }

        public virtual IList<Order> RetrieveAll()
        {
            var count = _orderRepository.RetrieveCount();
            var numberofpages = PagingFunctions.NumberOfPages(ShopifyOrderLimit, count);
            var results = new List<Order>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _logger.Debug(
                    string.Format(
                        "{3} - page {0} of {1} pages", pagenumber, numberofpages, this.ClassAndMethodName()));
                
                // This might throw an Error!!!
                var orders = _orderRepository.Retrieve(pagenumber, ShopifyOrderLimit);
                results.AddRange(orders);
            }

            TimeSpan ts = stopWatch.Elapsed;
            _logger.Debug(
                string.Format(
                    "OrderApiRepository.RetrieveAll total execution time {0} to fetch {1} Orders", 
                        ts.ToFormattedString(), results.Count));

            return results;
        }
    }
}

