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

            _pushLogger.Info($"{this.ClassAndMethodName()} - {results.Count} Orders to process");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var order in results)
            {
                _pushLogger.Debug($"Refresh Order: {order.Name} for {order.Email}");

                var shopifyOrder = order.ToShopifyOrder(shop.ShopId);

                orderRepository.DeleteOrder(shopifyOrder.ShopifyOrderId);
                orderRepository.InsertOrder(shopifyOrder);

                foreach (var line_item in shopifyOrder.LineItems)
                {
                    orderRepository.DeleteOrderLineItem(shopifyOrder.ShopifyOrderId);
                    orderRepository.InsertOrderLineItem(line_item);
                }
            }

            TimeSpan ts = stopWatch.Elapsed;
            _pushLogger.Info($"{this.ClassAndMethodName()} total execution time {ts.ToFormattedString()} to write {results.Count} Orders");
        }


        public virtual IList<Order> RetrieveAllFromShopify(ShopifyCredentials shopCredentials)
        {
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);

            // SEE THIS JEREMY??? NO MORE!!!
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            var count = orderApiRepository.RetrieveCount();
            _pushLogger.Info($"{this.ClassAndMethodName()} - {count} Orders to process");

            var numberofpages = 
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.MaxOrderRate, count);

            var results = new List<Order>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");
                
                // This might throw an Error!!!
                var orders = orderApiRepository.Retrieve(pagenumber, _refreshServiceConfiguration.MaxOrderRate);
                results.AddRange(orders);
            }

            // SEE THIS JEREMY??? NO MORE!!!
            //TimeSpan ts = stopWatch.Elapsed;
            //_pushLogger.Info($"{this.ClassAndMethodName()} total execution time {ts.ToFormattedString()} to fetch {results.Count} Orders");

            return results;
        }
    }
}

