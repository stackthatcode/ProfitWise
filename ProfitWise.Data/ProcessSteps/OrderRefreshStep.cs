using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.ShopifyImport;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class OrderRefreshStep
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopifyOrderDiagnosticShim _diagnostic;
        private readonly TimeZoneTranslator _timeZoneTranslator;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly PwShopRepository _shopRepository;

        // 60 minutes to account for daylight savings + 15 minutes to account for clock inaccuracies
        public const int MinutesFudgeFactor = 75;


        public OrderRefreshStep(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                PwShopRepository shopRepository,
                IPushLogger logger,
                ShopifyOrderDiagnosticShim diagnostic,
                TimeZoneTranslator timeZoneTranslator
            )

        {
            _pushLogger = logger;
            _diagnostic = diagnostic;
            _timeZoneTranslator = timeZoneTranslator;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerUserId);

            // Load Batch State
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var batchState = batchStateRepository.Retrieve();

            // CASE #1 - first time loading - don't do any additional updates
            if (batchState.OrderDatasetStart == null)
            {
                FirstTimeLoadWorker(shopCredentials, shop);
                return;
            }

            // CASE #2 - Order Dataset Start Date has moved back in time
            if (shop.StartingDateForOrders < batchState.OrderDatasetStart )
            {
                EarlierStartDateWorker(shopCredentials, shop);
            }

            // CASE #3 - update to get the latest Orders since last update
            RoutineUpdateWorker(shopCredentials, shop);
        }

        private void RoutineUpdateWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Routine Order refresh for {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            var updatedAtMinMachineTime = preBatchState.OrderDatasetEnd.Value;
            
            var updatedAtMinShopifyTime = 
                    _timeZoneTranslator.TranslateToTimeZone(updatedAtMinMachineTime, shop.TimeZone)
                            .AddMinutes(-MinutesFudgeFactor);

            
            var updatefilter = 
                new OrderFilter()
                {
                    UpdatedAtMin = updatedAtMinShopifyTime
                }
                .OrderByUpdateAtAscending();

            RefreshOrders(updatefilter, shopCredentials, shop);

            // Update Batch State End to now-ish
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetEnd = DateTime.Now.AddMinutes(-15); // Fudge factor for clock disparities
            batchStateRepository.Update(postBatchState);
            _pushLogger.Info("Complete: " + postBatchState.ToString());
        }

        private void EarlierStartDateWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Expanding Order date range for {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            var oldStartDateMachineTime = preBatchState.OrderDatasetStart.Value;
            var newStartingDateMachineTime = shop.StartingDateForOrders.Value;

            var createdAtMinShopifyTime =
                _timeZoneTranslator.TranslateToTimeZone(newStartingDateMachineTime, shop.TimeZone)
                    .AddMinutes(-MinutesFudgeFactor);

            var createdAtMaxShopifyTime =
                    _timeZoneTranslator.TranslateToTimeZone(oldStartDateMachineTime, shop.TimeZone)
                    .AddMinutes(MinutesFudgeFactor);

            var filter = new OrderFilter()
                {
                    CreatedAtMin = createdAtMinShopifyTime,
                    CreatedAtMax = createdAtMaxShopifyTime,
                }
                .OrderByCreatedAtDescending();

            RefreshOrders(filter, shopCredentials, shop);


            // When updating Batch State, simply move Start to Preferences' new Start
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = newStartingDateMachineTime;
            batchStateRepository.Update(postBatchState);
            _pushLogger.Info("Complete: " + postBatchState);
        }

        private void FirstTimeLoadWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Loading Orders first time for Shop {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();


            var createdAtMinMachineTime = shop.StartingDateForOrders.Value;

            var createdAtMinShopifyTime =
                    _timeZoneTranslator.TranslateToTimeZone(createdAtMinMachineTime, shop.TimeZone)
                            .AddMinutes(-MinutesFudgeFactor);

            var filter = new OrderFilter()
                {
                    CreatedAtMin = createdAtMinShopifyTime
                }.OrderByCreatedAtDescending();


            // Set the Batch State Order End point to now... 
            preBatchState.OrderDatasetEnd = DateTime.Now;
            batchStateRepository.Update(preBatchState);

            // ... then walk backward through time
            RefreshOrders(filter, shopCredentials, shop);


            // Update Post Batch State so Start Date equals that which is in Preferences
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = shop.StartingDateForOrders;
            batchStateRepository.Update(postBatchState);
            _pushLogger.Info("Complete: " + postBatchState);
        }



        // All the "worker" functions call Refresh Orders main function
        private void RefreshOrders(OrderFilter filter, ShopifyCredentials shopCredentials, PwShop shop)
        {
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);

            var count = orderApiRepository.RetrieveCount(filter);
            _pushLogger.Info($"{count} Orders to process ({filter})");

            var numberofpages =
                    PagingFunctions.NumberOfPages(
                        _refreshServiceConfiguration.MaxOrderRate, count);

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages");
                var importedOrders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);

                WriteOrdersToPersistence(importedOrders, shop);

                // Update the Batch State based on Order Filter's Sort
                var batchState = batchStateRepository.Retrieve();
                if (filter.ShopifySortOrder == ShopifySortOrder.Ascending)
                {
                    batchState.OrderDatasetEnd = importedOrders.Max(x => x.UpdatedAt);
                }
                else
                {
                    batchState.OrderDatasetStart = importedOrders.Min(x => x.CreatedAt);
                }
                batchStateRepository.Update(batchState);
            }
        }

        protected virtual void WriteOrdersToPersistence(IList<Order> importedOrders, PwShop shop)
        {
            var catalogBuilderService = _multitenantFactory.MakeCatalogBuilderService(shop);
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(shop);

            _pushLogger.Info($"{importedOrders.Count} Orders to process");

            using (var trans = orderRepository.InitiateTransaction())
            {
                var masterProductCatalog = catalogBuilderService.RetrieveFullCatalog();                
                var orderIdList = importedOrders.Select(x => x.Id).ToList();
                var existingOrders = orderRepository.RetrieveOrdersFullDepth(orderIdList);

                var context = new OrderRefreshContext
                {
                    PwShop = shop,
                    MasterProducts = masterProductCatalog,
                    CurrentExistingOrders = existingOrders,
                };
                
                foreach (var importedOrder in importedOrders)
                {
                    WriteOrderToPersistence(importedOrder, context);
                }

                //cogsRepostory.UpdateOrderLinesWithSimpleCogs();
                trans.Commit();
            }
        }

        private void WriteOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var existingOrder =
                context.CurrentExistingOrders
                    .FirstOrDefault(x => x.ShopifyOrderId == orderFromShopify.Id);

            if (_diagnostic.PwShopId == context.PwShop.PwShopId && _diagnostic.OrderIds.Contains(orderFromShopify.Id))
            {
                _pushLogger.Debug(orderFromShopify.ToString());
            }

            //if (existingOrder == null && orderFromShopify.Cancelled == true)
            //{
            //    _pushLogger.Debug($"Skipping cancelled Order: {orderFromShopify.Name}/{orderFromShopify.Id}");
            //    return;
            //}

            //if (existingOrder != null && orderFromShopify.Cancelled == true)
            //{
            //    _pushLogger.Debug($"Deleting cancelled Order: {orderFromShopify.Name}/{orderFromShopify.Id}");                
            //    orderRepository.DeleteOrderFullDepth(orderFromShopify.Id);
            //    return;
            //}

            _pushLogger.Debug($"Translating Order from Shopify {orderFromShopify.Name}/{orderFromShopify.Id} to ProfitWise data model");

            if (existingOrder == null)
            {
                InsertOrderToPersistence(orderFromShopify, context);
            }
            else
            {
                UpdateOrderToPersistence(orderFromShopify, existingOrder, context);
            }                 
        }

        public void InsertOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var translatedOrder = orderFromShopify.ToShopifyOrder(context.PwShop.PwShopId);

            _pushLogger.Debug($"Inserting new Order: {orderFromShopify.Name}/{orderFromShopify.Id}");
            _pushLogger.Trace(Environment.NewLine + translatedOrder.ToString());

            foreach (var lineItemFromShopify in orderFromShopify.LineItems)
            {
                var translatedLineItem =
                        translatedOrder
                            .LineItems.First(x => x.ShopifyOrderLineId == lineItemFromShopify.Id);

                var pwVariant = FindCreateProductVariant(context, lineItemFromShopify);
                translatedLineItem.SetProfitWiseVariant(pwVariant);
            }

            orderRepository.InsertOrder(translatedOrder);

            foreach (var item in translatedOrder.LineItems)
            {
                _pushLogger.Debug($"Inserting new Order Line Item: {item.ShopifyOrderLineId}");                
                orderRepository.InsertLineItem(item);

                foreach (var refund in item.Refunds)
                {
                    _pushLogger.Debug($"Inserting new Refund: {refund.ShopifyRefundId}");
                    orderRepository.InsertRefund(refund);
                }
            }

            foreach (var adjustment in translatedOrder.Adjustments)
            {
                _pushLogger.Debug($"Inserting new Order Adjustment: {adjustment.ShopifyAdjustmentId}");
                orderRepository.InsertAdjustment(adjustment);
            }
        }

        public void UpdateOrderToPersistence(
                Order orderFromShopify, ShopifyOrder existingOrder, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var importedOrder = orderFromShopify.ToShopifyOrder(context.PwShop.PwShopId);
            
            _pushLogger.Debug($"Updating existing Order: {importedOrder.OrderNumber}/{importedOrder.ShopifyOrderId}");
            orderRepository.UpdateOrder(importedOrder);

            foreach (var importedLineItem in importedOrder.LineItems)
            {
                _pushLogger.Debug($"Updating existing Line Item Net Total: {importedLineItem.ShopifyOrderLineId}");
                orderRepository.UpdateLineItemNetTotal(importedLineItem);

                var existingLineItem =
                    existingOrder.LineItems.First(x => x.ShopifyOrderLineId == importedLineItem.ShopifyOrderLineId);

                foreach (var refund in importedLineItem.Refunds)
                {
                    if (existingLineItem.Refunds.Any(x => x.ShopifyRefundId == refund.ShopifyRefundId))
                    {
                        continue;
                    }
                    orderRepository.InsertRefund(refund);
                }
            }

            foreach (var adjustment in importedOrder.Adjustments)
            {
                if (existingOrder.Adjustments.Any(x => x.ShopifyAdjustmentId == adjustment.ShopifyAdjustmentId))
                {
                    continue;
                }
                orderRepository.InsertAdjustment(adjustment);
            }
        }

        public PwVariant FindCreateProductVariant(OrderRefreshContext context, OrderLineItem importedLineItem)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(context.PwShop);

            var masterProduct =
                context.MasterProducts.FindMasterProduct(
                    importedLineItem.ProductTitle, importedLineItem.Vendor);

            if (masterProduct == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Product for Title: {importedLineItem.ProductTitle} " +
                    $"and Vendor: {importedLineItem.Vendor}");
                masterProduct = service.BuildAndSaveMasterProduct();
            }

            context.MasterProducts.Add(masterProduct);

            var product = masterProduct.FindProduct(
                importedLineItem.ProductTitle, importedLineItem.Vendor, importedLineItem.ProductId);

            if (product == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Product for Title: {importedLineItem.ProductTitle} " +
                    $"and Vendor: {importedLineItem.Vendor} and " +
                    $"Shopify Id: {importedLineItem.ProductId}");

                product =
                    service.BuildAndSaveProduct(
                        masterProduct, false, importedLineItem.ProductTitle, importedLineItem.ProductId,
                        importedLineItem.Vendor, "", "");
            }

            var masterVariant =
                masterProduct.FindMasterVariant(importedLineItem.Sku, importedLineItem.VariantTitle);

            if (masterVariant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Variant for Title: {importedLineItem.VariantTitle} " +
                    $"and Sku: {importedLineItem.Sku}");

                masterVariant =
                    service.BuildAndSaveMasterVariant(
                        product, importedLineItem.VariantTitle, importedLineItem.ProductId,
                        importedLineItem.VariantId, importedLineItem.Sku);
                masterProduct.MasterVariants.Add(masterVariant);
            }

            var variant = masterVariant.FindVariant(importedLineItem.Sku, importedLineItem.VariantTitle, importedLineItem.VariantId);

            if (variant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Variant for Title: {importedLineItem.VariantTitle} " +
                    $"and Sku: {importedLineItem.Sku} and Shopify Variant Id: {importedLineItem.VariantId}");
                variant =
                    service.BuildAndSaveVariant(
                        masterVariant, false, product, importedLineItem.VariantTitle, importedLineItem.VariantId, importedLineItem.Sku);
            }

            return variant;
        }        
    }
}

