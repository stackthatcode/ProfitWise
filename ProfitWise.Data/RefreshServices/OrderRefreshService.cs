using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;
using Push.Utilities.Logging;

namespace ProfitWise.Data.RefreshServices
{
    public class OrderRefreshService
    {
        private readonly ILogger _logger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly SqlRepositoryFactory _sqlRepositoryFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;


        public OrderRefreshService(
                ApiRepositoryFactory apiRepositoryFactory,
                SqlRepositoryFactory sqlRepositoryFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ILogger logger)

        {
            _logger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _sqlRepositoryFactory = sqlRepositoryFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            var results = RetrieveAll(shopCredentials);

            // TODO - write the actual data to SQL
            foreach (var order in results)
            {
                var message = $"Order found: {order.Id} - {order.Email}";
                _logger.Debug(message);
            }
        }


        public virtual IList<Order> RetrieveAll(ShopifyCredentials shopCredentials)
        {
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);

            var count = orderApiRepository.RetrieveCount();
            var numberofpages = 
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.RefreshServiceMaxOrderRate, count);

            var results = new List<Order>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _logger.Debug(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");
                
                // This might throw an Error!!!
                var orders = orderApiRepository.Retrieve(pagenumber, _refreshServiceConfiguration.RefreshServiceMaxOrderRate);
                results.AddRange(orders);
            }

            TimeSpan ts = stopWatch.Elapsed;
            _logger.Debug(
                $"OrderApiRepository.RetrieveAll total execution time {ts.ToFormattedString()} to fetch {results.Count} Orders");

            return results;
        }
    }
}

