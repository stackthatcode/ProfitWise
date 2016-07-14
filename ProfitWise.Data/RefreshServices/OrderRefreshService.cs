using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Batch.RefreshServices;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.RefreshServices
{
    public class OrderRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopifyOrderDiagnosticShim _diagnostic;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly ShopRepository _shopRepository;


        public OrderRefreshService(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ShopRepository shopRepository,
                IPushLogger logger,
                ShopifyOrderDiagnosticShim diagnostic)

        {
            _pushLogger = logger;
            _diagnostic = diagnostic;
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
                UpdatedAtMin = batchState.OrderDatasetEnd.Value.AddMinutes(-30)
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
            _pushLogger.Info($"{count} Orders to process");

            var numberofpages =
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.MaxOrderRate, count);

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"Page {pagenumber} of {numberofpages} pages");

                var orders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);
                //var testOrder = orders.FirstOrDefault(x => x.Id == 3347193029);
                WriteOrdersToPersistence(shop, orders);
            }
        }


        protected virtual void WriteOrdersToPersistence(ShopifyShop shop, IList<Order> ordersFromShopify)
        {
            var orderRepository = _multitenantRepositoryFactory.MakeShopifyOrderRepository(shop);

            _pushLogger.Info($"{ordersFromShopify.Count} Orders to process");

            using (var trans = orderRepository.InitiateTransaction())
            {
                var importedShopifyOrders = new List<ShopifyOrder>();
                foreach (var order in ordersFromShopify)
                {
                    _pushLogger.Debug($"Translating Shopify Order {order.Name} ({order.Id}) to ProfitWise data model");
                    importedShopifyOrders.Add(order.ToShopifyOrder(shop.ShopId));
                }

                var orderIdList = importedShopifyOrders.Select(x => x.ShopifyOrderId).ToList();
                var existingShopifyOrders = orderRepository.RetrieveOrders(orderIdList);
                var existingShopifyOrderLineItems = orderRepository.RetrieveOrderLineItems(orderIdList);
                existingShopifyOrders.LoadLineItems(existingShopifyOrderLineItems);

                foreach (var importedOrder in importedShopifyOrders)
                {
                    WriteOrderToPersistence(existingShopifyOrders, importedOrder, orderRepository, existingShopifyOrderLineItems);
                }

                trans.Commit();
            }
        }



        private void WriteOrderToPersistence(IList<ShopifyOrder> existingShopifyOrders, ShopifyOrder importedOrder,
            ShopifyOrderRepository orderRepository, IList<ShopifyOrderLineItem> existingShopifyOrderLineItems)
        {
            if (_diagnostic.ShopId == importedOrder.ShopId &&
                _diagnostic.OrderIds.Contains(importedOrder.ShopifyOrderId))
            {
                _pushLogger.Debug(importedOrder.ToString());
            }
            
            var existingOrder =
                existingShopifyOrders.FirstOrDefault(
                    x => x.ShopifyOrderId == importedOrder.ShopifyOrderId);

            if (existingOrder == null)
            {
                _pushLogger.Debug(
                    $"Inserting new Order: {importedOrder.OrderNumber} / {importedOrder.ShopifyOrderId} for {importedOrder.Email}");

                orderRepository.InsertOrder(importedOrder);
            }
            else
            {
                _pushLogger.Debug(
                    $"Updating existing Order: {importedOrder.OrderNumber} / {importedOrder.ShopifyOrderId} for {importedOrder.Email}");

                existingOrder.UpdatedAt = importedOrder.UpdatedAt;
                existingOrder.Email = importedOrder.Email;
                existingOrder.TotalRefund = importedOrder.TotalRefund;
                existingOrder.TaxRefundAmount = importedOrder.TaxRefundAmount;
                existingOrder.ShippingRefundAmount = importedOrder.ShippingRefundAmount;
                orderRepository.UpdateOrder(existingOrder);
            }

            foreach (var importedLineItem in importedOrder.LineItems)
            {
                var existingLineItem =
                    existingShopifyOrderLineItems.FirstOrDefault(
                        x => x.ShopifyOrderId == importedLineItem.ShopifyOrderId && 
                            x.ShopifyOrderLineId == importedLineItem.ShopifyOrderLineId);

                if (existingLineItem == null)
                {
                    _pushLogger.Debug(
                        $"Inserting new Order Line Item: {importedOrder.OrderNumber} / {importedOrder.ShopifyOrderId} / {importedLineItem.ShopifyOrderLineId}");
                    orderRepository.InsertOrderLineItem(importedLineItem);
                }
                else
                {
                    _pushLogger.Debug(
                        $"Updating existing Order Line Item: {importedOrder.OrderNumber} / {importedOrder.ShopifyOrderId} / {importedLineItem.ShopifyOrderLineId}");

                    existingLineItem.TotalRestockedQuantity = importedLineItem.TotalRestockedQuantity;
                    existingLineItem.GrossRevenue = importedLineItem.GrossRevenue;
                    orderRepository.UpdateOrderLineItem(existingLineItem);
                }
            }
        }
    }
}

