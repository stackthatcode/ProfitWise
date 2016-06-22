using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
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
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantSqlRepositoryFactory _multitenantSqlRepositoryFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly ShopRepository _shopRepository;


        public OrderRefreshService(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantSqlRepositoryFactory multitenantSqlRepositoryFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ShopRepository shopRepository,
                IPushLogger logger)

        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantSqlRepositoryFactory = multitenantSqlRepositoryFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            var results = RetrieveAllFromShopify(shopCredentials);

            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);
            WriteOrdersToPersistence(shop, results);
        }

        protected virtual void WriteOrdersToPersistence(ShopifyShop shop, IList<Order> results)
        {
            var orderRepository = _multitenantSqlRepositoryFactory.MakeOrderRepository(shop);

            foreach (var order in results)
            {
                var shopifyOrder = order.ToShopifyOrder(shop.ShopId);
                orderRepository.DeleteOrder(shopifyOrder.ShopifyOrderId);
                orderRepository.InsertOrder(shopifyOrder);

                var message = $"Refreshing Order: {shopifyOrder.OrderNumber} for {shopifyOrder.Email}";
                _pushLogger.Debug(message);
            }
        }


        public virtual IList<Order> RetrieveAllFromShopify(ShopifyCredentials shopCredentials)
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
                _pushLogger.Debug(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");
                
                // This might throw an Error!!!
                var orders = orderApiRepository.Retrieve(pagenumber, _refreshServiceConfiguration.RefreshServiceMaxOrderRate);
                results.AddRange(orders);
            }

            TimeSpan ts = stopWatch.Elapsed;
            _pushLogger.Debug(
                $"OrderApiRepository.RetrieveAll total execution time {ts.ToFormattedString()} to fetch {results.Count} Orders");

            return results;
        }
    }
}

