using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MySql.Data.MySqlClient;
using ProfitWise.Batch.Factory;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Shopify.Repositories;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Batch.Orders
{
    public class OrderRefreshService
    {
        private readonly string _userId;

        private readonly OrderApiRepository _orderRepository;
        private readonly ILogger _logger;
        private readonly MySqlConnection _connection;


        public OrderRefreshService(string userId, ILogger logger, IShopifyClientFactory shopifyClientFactory)
        {
            _userId = userId;
            _logger = logger;

            var shopifyClient = shopifyClientFactory.Make(userId);
            _orderRepository = new OrderApiRepository(shopifyClient, logger);

            _connection = MySqlConnectionFactory.Make();
        }

        public void Execute()
        {
            var results = RetrieveAll();

            foreach (var order in results)
            {
                var message = string.Format("Order found: {0} - {1}", order.Id, order.Email);
                _logger.Info(message);
            }
        }

        public IList<Order> RetrieveAll(int limit = 250, int delay = 500)
        {
            var count = _orderRepository.RetrieveCount();
            var numberofpages = PagingFunctions.NumberOfPages(limit, count);
            var results = new List<Order>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _logger.Debug(
                    string.Format(
                        "OrderApiRepository->RetrieveAll() - page {0} of {1} pages", pagenumber, numberofpages));

                Stopwatch shopifyRetrieveStopWatch = new Stopwatch();
                shopifyRetrieveStopWatch.Start();

                // This might throw an Error!!!
                var orders = _orderRepository.Retrieve(pagenumber, limit);

                TimeSpan shopifyRetrieveTimeElapsed = shopifyRetrieveStopWatch.Elapsed;
                string elapsedTimeInner = shopifyRetrieveTimeElapsed.ToFormattedString();

                _logger.Debug("Retrieve call performance... " + elapsedTimeInner);

                results.AddRange(orders);

                TimeSpan delayLength = new TimeSpan(0, 0, 0, 0, delay);

                if (shopifyRetrieveTimeElapsed < delayLength)
                {
                    TimeSpan remainingTimeToDelay = delayLength - shopifyRetrieveTimeElapsed;
                    _logger.Debug("Intentional delay: " + remainingTimeToDelay.ToFormattedString());
                    Thread.Sleep(remainingTimeToDelay);
                }
            }

            TimeSpan ts = stopWatch.Elapsed;
            _logger.Debug("End... " + ts.ToFormattedString());

            return results;
        }


    }
}

