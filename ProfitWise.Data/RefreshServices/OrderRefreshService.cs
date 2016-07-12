using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.General;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.RefreshServices
{
    public class OrderRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly ShopRepository _shopRepository;


        public OrderRefreshService(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ShopRepository shopRepository,
                IPushLogger logger)

        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantRepositoryFactory = multitenantRepositoryFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Load Batch State
            var batchStateRepository = _multitenantRepositoryFactory.MakeBatchStateRepository(shop);
            var batchState = batchStateRepository.Retrieve();

            // Get User Preferences
            var preferencesRepository = _multitenantRepositoryFactory.MakePreferencesRepository(shop);
            var preferences = preferencesRepository.Retrieve();

            // First Time Loading
            if (batchState.OrderDatasetStart == null)
            {
                _pushLogger.Info($"Loading Orders first time for Shop {shop.ShopId}");
                var filter = new OrderFilter()
                {
                    CreatedAtMin = preferences.StartingDateForOrders
                };
                RefreshOrders(filter, shopCredentials, shop);

                batchState.OrderDatasetStart = preferences.StartingDateForOrders;
                batchState.OrderDatasetEnd = DateTime.Now.AddMinutes(-15);  // Fudge factor for clock disparities

                batchStateRepository.Update(batchState);
                _pushLogger.Info(batchState.ToString());
                return;
            }

            // Order Dataset Start Date has moved back in time
            if (preferences.StartingDateForOrders < batchState.OrderDatasetStart )
            {
                _pushLogger.Info($"Expanded Order date range for {shop.ShopId}");

                var filter = new OrderFilter()
                {
                    CreatedAtMin = preferences.StartingDateForOrders,
                    CreatedAtMax = batchState.OrderDatasetStart,
                };
                RefreshOrders(filter, shopCredentials, shop);

                batchState.OrderDatasetStart = preferences.StartingDateForOrders;
                batchStateRepository.Update(batchState);
                _pushLogger.Info(batchState.ToString());
            }

            // Update to get the latest Orders
            _pushLogger.Info($"Routine Order refresh for {shop.ShopId}");

            var updatefilter = new OrderFilter()
            {
                UpdatedAtMin = batchState.ProductsLastUpdated
            };

            RefreshOrders(updatefilter, shopCredentials, shop);
            batchState.OrderDatasetEnd = DateTime.Now.AddMinutes(-15);  // Fudge factor for clock disparities
            batchStateRepository.Update(batchState);
            _pushLogger.Info(batchState.ToString());
        }


        private void RefreshOrders(OrderFilter filter, ShopifyCredentials shopCredentials, ShopifyShop shop)
        {
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);
            var count = orderApiRepository.RetrieveCount(filter);
            _pushLogger.Info($"{this.ClassAndMethodName()} - {count} Orders to process");

            var numberofpages =
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.MaxOrderRate, count);

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"{this.ClassAndMethodName()} - page {pagenumber} of {numberofpages} pages");

                var orders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);

                WriteOrdersToPersistence(shop, orders);
            }
        }


        protected virtual void WriteOrdersToPersistence(ShopifyShop shop, IList<Order> results)
        {
            var orderRepository = _multitenantRepositoryFactory.MakeShopifyOrderRepository(shop);

            _pushLogger.Info($"{this.ClassAndMethodName()} - {results.Count} Orders to process");

            using (var trans = orderRepository.InitiateTransaction())
            {
                var shopifyOrders = results.Select(x => x.ToShopifyOrder(shop.ShopId)).ToList();

                orderRepository.MassDelete(shopifyOrders);

                foreach (var order in shopifyOrders)
                {
                    _pushLogger.Debug($"Refresh Order: {order.OrderNumber} / {order.ShopifyOrderId} for {order.Email}");

                    orderRepository.InsertOrder(order);

                    foreach (var line_item in order.LineItems)
                    {
                        orderRepository.InsertOrderLineItem(line_item);
                    }
                }

                trans.Commit();
            }
        }

    }
}

