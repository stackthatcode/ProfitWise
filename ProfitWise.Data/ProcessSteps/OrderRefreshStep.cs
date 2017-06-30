using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class OrderRefreshStep
    {
        private readonly BatchLogger _pushLogger;
        private readonly TimeZoneTranslator _timeZoneTranslator;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly ShopRepository _shopRepository;

        // This is to account for a whole range of possibilities, from clock offsets,
        // ... to the Shop's Timezone changing
        public const int OrderStartFudgeFactor = -75;
        public const int OrderEndFudgeFactor = 75;

        public OrderRefreshStep(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                ShopRepository shopRepository,
                BatchLogger logger,
                TimeZoneTranslator timeZoneTranslator)
        {
            _pushLogger = logger;
            _timeZoneTranslator = timeZoneTranslator;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }
        

        public void Execute(ShopifyCredentials shopCredentials)
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
            var preferenceStartDateUtc = shop.StartingDateForOrders.ToUtcFromShopTz(shop.TimeZone);
            if (preferenceStartDateUtc < batchState.OrderDatasetStart)
            {
                EarlierStartDateWorker(shopCredentials, shop);
            }

            // CASE #3 - update to get the latest Orders since last update
            RoutineUpdateWorker(shopCredentials, shop);
        }

        public void ExecuteSingleOrder(ShopifyCredentials shopifyCredentials, long orderId)
        {
            var shop = _shopRepository.RetrieveByUserId(shopifyCredentials.ShopOwnerUserId);
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopifyCredentials);
            var order = orderApiRepository.Retrieve(orderId);
            WriteOrdersToPersistence(new List<Order> { order }, shop);
        }
        
        private void RoutineUpdateWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Routine Order Refresh");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            // The Batch State stores everything in UTC, thus we need to translate to the Shop's Timezone
            var updatedAtMinUtc = preBatchState.OrderDatasetEnd.Value;  
            var processStartUtc = DateTime.UtcNow;

            var updatefilter = new OrderFilter
                {
                    UpdatedAtMinUtc = updatedAtMinUtc.AddMinutes(OrderStartFudgeFactor)
                }
                .OrderByUpdateAtAscending();

            RefreshOrders(updatefilter, shopCredentials, shop);

            // Update Batch State End to now-ish
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetEnd = processStartUtc;
            batchStateRepository.Update(postBatchState);

            _pushLogger.Info("Complete: " + postBatchState);
        }
        
        private void EarlierStartDateWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Expanding Order date range for {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            // Preferences -> Starting Date for Orders stores a "pure" Date i.e. Timezone unspecified, 
            // ... that's implied to exist in the Shop's Timezone
            var startDateFromPreferencesUtc = shop.StartingDateForOrders.ToUtcFromShopTz(shop.TimeZone);
            var oldStartDateUtc = preBatchState.OrderDatasetStart.Value;

            // ** Critical => OrderEndFudgeFactor accounts for any changes to Timezone
            var filter = new OrderFilter()
                {
                    UpdatedAtMinUtc = startDateFromPreferencesUtc.AddMinutes(OrderStartFudgeFactor),
                    UpdatedAtMaxUtc = oldStartDateUtc.AddMinutes(OrderEndFudgeFactor),
                }
                .OrderByUpdateAtDescending();

            RefreshOrders(filter, shopCredentials, shop);

            // When updating Batch State, simply move Start to Preferences' new Start in UTC
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = startDateFromPreferencesUtc;
            batchStateRepository.Update(postBatchState);
            
            _pushLogger.Info("Complete: " + postBatchState);
        }

        private void FirstTimeLoadWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Loading Orders first time for Shop {shop.PwShopId}");
            
            // Translate the Starting Date for Orders to Shop Timezone for the API invocation
            var startDateShopUtc = shop.StartingDateForOrders.ToUtcFromShopTz(shop.TimeZone);
            var endDateShopUtc = DateTime.UtcNow;
            
            var filter = new OrderFilter
                {                
                    UpdatedAtMinUtc = startDateShopUtc.AddMinutes(OrderStartFudgeFactor),
                    UpdatedAtMaxUtc = endDateShopUtc.AddMinutes(OrderEndFudgeFactor),
                }
                .OrderByUpdateAtDescending();

            // The Batch State stores everything in UTC, thus we need to translate to the Shop's Time Zone
            // Set the Batch State Order End point to UTC now... 
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();
            preBatchState.OrderDatasetEnd = endDateShopUtc;
            batchStateRepository.Update(preBatchState);

            // ... then walk backward through time
            RefreshOrders(filter, shopCredentials, shop);

            // Update Post Batch State so Start Date in UTC that equals that which is in Preferences
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = startDateShopUtc;
            batchStateRepository.Update(postBatchState);

            _pushLogger.Info("Complete: " + postBatchState);
        }

        // All the "worker" functions call Refresh Orders main function
        public void RefreshOrders(OrderFilter filter, ShopifyCredentials shopCredentials, PwShop shop)
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
                var importedOrders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages - {importedOrders.Count} Orders to process");

                WriteOrdersToPersistence(importedOrders, shop);

                // Update the Batch State based on Order Filter's Sort
                var batchState = batchStateRepository.Retrieve();
                if (filter.ShopifySortOrder == ShopifySortOrder.Ascending)
                {
                    var latestOrderUpdatedShopTz = importedOrders.Max(x => x.UpdatedAtShopTz);
                    batchState.OrderDatasetEnd = latestOrderUpdatedShopTz.ToUtcFromShopTz(shop.TimeZone);
                }
                else
                {
                    var earliestOrderCreatedShopTz = importedOrders.Min(x => x.CreatedAtShopTz);
                    batchState.OrderDatasetStart = earliestOrderCreatedShopTz.ToUtcFromShopTz(shop.TimeZone);
                }
                batchStateRepository.Update(batchState);
            }
        }

        public void WriteOrdersToPersistence(IList<Order> importedOrders, PwShop shop)
        {
            var catalogBuilderService = _multitenantFactory.MakeCatalogRetrievalService(shop);
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(shop);
            
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
        }

        private void WriteOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDownstreamRepository(context.PwShop);

            var existingOrder =
                context.CurrentExistingOrders
                    .FirstOrDefault(x => x.ShopifyOrderId == orderFromShopify.Id);            
            
            var repository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);

            using (var transaction = repository.InitiateTransaction())
            {
                if (existingOrder == null)
                {
                    InsertOrderToPersistence(orderFromShopify, context);
                }
                else
                {
                    UpdateOrderToPersistence(orderFromShopify, existingOrder, context);
                }

                var refreshContext = new EntryRefreshContext() { ShopifyOrderId = orderFromShopify.Id };
                cogsUpdateRepository.DeleteEntryLedger(refreshContext);

                // Important Rule: ProfitWise does not maintain Ledge Entries for Orders that are:
                // ... both Cancelled and with Payment that is Not Captured
                if (!(orderFromShopify.Cancelled)) // && !orderFromShopify.FinancialStatus.PaymentCaptured()))
                {
                    cogsUpdateRepository.RefreshEntryLedger(refreshContext);
                }

                transaction.Commit();
            }
        }

        public void InsertOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var cogsService = _multitenantFactory.MakeCogsService(context.PwShop);

            _pushLogger.Debug($"Inserting new Order: {orderFromShopify.Name}/{orderFromShopify.Id}");

            var translatedOrder = orderFromShopify.ToShopifyOrder(context.PwShop, _timeZoneTranslator);
            _pushLogger.Trace($"BalancingCorrection: {translatedOrder.BalancingCorrection}");

            foreach (var lineItem in orderFromShopify.LineItems)
            {
                var translatedLineItem = translatedOrder.LineItems.First(x => x.ShopifyOrderLineId == lineItem.Id);

                if (_pushLogger.IsTraceEnabled)
                {
                    _pushLogger.Trace(translatedLineItem.ToString());
                }

                var productBuildContext = lineItem.ToProductBuildContext(context.MasterProducts);
                var variantBuildContext = lineItem.ToVariantBuildContext(allMasterProducts: context.MasterProducts);

                var pwVariant = FindCreateProductVariant(context.PwShop, productBuildContext, variantBuildContext);
                _pushLogger.Trace("Matching Variant: " + pwVariant.ToString());

                translatedLineItem.SetProfitWiseVariant(pwVariant);

                // In-memory CoGS computation
                var cogsContexts = CogsDateBlockContext.Make(pwVariant.ParentMasterVariant, context.PwShop.CurrencyId);

                if (_pushLogger.IsTraceEnabled)
                {
                    foreach (var cogs in cogsContexts)
                    {
                        _pushLogger.Trace(cogs.ToString());
                    }
                }

                var unitCogs = cogsService.AssignUnitCogsToLineItem(cogsContexts, translatedLineItem);
                translatedLineItem.UnitCogs = unitCogs;

                _pushLogger.Trace(
                    $"Computed CoGS for new Order Line Item: {translatedLineItem.ShopifyOrderLineId}  - {unitCogs}");
            }
            _pushLogger.Trace(Environment.NewLine + translatedOrder);
            
            orderRepository.InsertOrder(translatedOrder);

            foreach (var item in translatedOrder.LineItems)
            {
                _pushLogger.Trace($"Inserting new Order Line Item: {item.ShopifyOrderLineId}");                
                orderRepository.InsertLineItem(item);

                foreach (var refund in item.Refunds)
                {
                    _pushLogger.Trace($"Inserting new Refund: {refund.ShopifyRefundId}");
                    orderRepository.InsertRefund(refund);
                }
            }

            foreach (var adjustment in translatedOrder.Adjustments)
            {
                _pushLogger.Trace($"Inserting new Order Adjustment: {adjustment.ShopifyAdjustmentId}");
                orderRepository.InsertAdjustment(adjustment);
            }
        }

        public void UpdateOrderToPersistence(
                Order orderFromShopify, ShopifyOrder existingOrder, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var importedOrder = orderFromShopify.ToShopifyOrder(context.PwShop, _timeZoneTranslator);
            
            _pushLogger.Debug($"Updating existing Order: {importedOrder.OrderNumber}/{importedOrder.ShopifyOrderId}");
            orderRepository.UpdateOrder(importedOrder);

            _pushLogger.Debug($"BalancingCorrection: {importedOrder.BalancingCorrection}");

            foreach (var importedLineItem in importedOrder.LineItems)
            {
                _pushLogger.Debug($"Updating existing Line Item Net Total: {importedLineItem.ShopifyOrderLineId}");
                orderRepository.UpdateLineItemNetTotalAndStatus(importedLineItem);

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

        public PwVariant FindCreateProductVariant(
                PwShop shop, ProductBuildContext productBuildContext, VariantBuildContext variantBuildContext)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);
            
            var masterProduct = productBuildContext.MasterProducts.FindMasterProduct(productBuildContext);
            if (masterProduct == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Product for Title: {productBuildContext.Title} " +
                    $"and Vendor: {productBuildContext.Vendor}");

                masterProduct = service.CreateMasterProduct();
                productBuildContext.MasterProducts.Add(masterProduct);
            }

            variantBuildContext.Product = masterProduct.FindProduct(productBuildContext);            
            if (variantBuildContext.Product == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Product for Title: {productBuildContext.Title} " +
                    $"and Vendor: {productBuildContext.Vendor} and " +
                    $"Shopify Id: {productBuildContext.ShopifyProductId}");

                variantBuildContext.Product = service.CreateProduct(masterProduct, productBuildContext);
            }

            variantBuildContext.MasterVariant = masterProduct.FindMasterVariant(variantBuildContext);            
            if (variantBuildContext.MasterVariant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Variant for Title: {variantBuildContext.Title} " +
                    $"and Sku: {variantBuildContext.Sku}");

                variantBuildContext.MasterVariant = service.CreateMasterVariant(variantBuildContext);
                masterProduct.MasterVariants.Add(variantBuildContext.MasterVariant);
            }

            var variant = variantBuildContext.MasterVariant.FindVariant(variantBuildContext);            
            if (variant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Variant for Title: {variantBuildContext.Title} " +
                    $"and Sku: {variantBuildContext.Sku} " +
                    $"and Shopify Variant Id: {variantBuildContext.ShopifyVariantId}");
                variant = service.CreateVariant(variantBuildContext);
            }

            return variant;
        }        
    }
}

