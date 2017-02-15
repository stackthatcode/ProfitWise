using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Model.ShopifyImport;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Data.Utility;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class OrderRefreshStep
    {
        private readonly BatchLogger _pushLogger;
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
                BatchLogger logger,
                ShopifyOrderDiagnosticShim diagnostic,
                TimeZoneTranslator timeZoneTranslator)
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
            _pushLogger.Info($"Routine Order Refresh");

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
            _pushLogger.Info("Complete: " + postBatchState);
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
                    ProcessedAtMin = createdAtMinShopifyTime,
                    ProcessedAtMax = createdAtMaxShopifyTime,
                }
                .OrderByProcessAtDescending();

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
                    ProcessedAtMin = createdAtMinShopifyTime
                }.OrderByProcessAtDescending();


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
                var importedOrders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages - {importedOrders.Count} Orders to process");

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
            var catalogBuilderService = _multitenantFactory.MakeCatalogRetrievalService(shop);
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(shop);
            var cogsUpdateRepository = _multitenantFactory.MakeCogsDownstreamRepository(shop);

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
                
            cogsUpdateRepository.RefreshReportEntryData();            
        }

        private void WriteOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var existingOrder =
                context.CurrentExistingOrders
                    .FirstOrDefault(x => x.ShopifyOrderId == orderFromShopify.Id);

            if (_diagnostic.PwShopId == context.PwShop.PwShopId && _diagnostic.OrderIds.Contains(orderFromShopify.Id))
            {
                _pushLogger.Debug(orderFromShopify.ToString());
            }

            // If the Order is Void then Delete the Order            
            _pushLogger.Debug(
                $"Translating Order from Shopify {orderFromShopify.Name}/{orderFromShopify.Id} to ProfitWise data model");

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

                repository.CommitTransaction();
            }
        }

        public void InsertOrderToPersistence(Order orderFromShopify, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.PwShop);
            var cogsService = _multitenantFactory.MakeCogsUpdateService(context.PwShop);

            var translatedOrder = orderFromShopify.ToShopifyOrder(context.PwShop.PwShopId);

            _pushLogger.Debug($"Inserting new Order: {orderFromShopify.Name}/{orderFromShopify.Id}");
            _pushLogger.Trace(Environment.NewLine + translatedOrder.ToString());

            foreach (var lineItem in orderFromShopify.LineItems)
            {
                var translatedLineItem = translatedOrder.LineItems.First(x => x.ShopifyOrderLineId == lineItem.Id);

                var productBuildContext = lineItem.ToProductBuildContext(context.MasterProducts);
                var variantBuildContext = lineItem.ToVariantBuildContext(allMasterProducts: context.MasterProducts);

                var pwVariant = FindCreateProductVariant(context.PwShop, productBuildContext, variantBuildContext);
                translatedLineItem.SetProfitWiseVariant(pwVariant);

                // In-memory CoGS computation
                var cogsContexts = 
                    CogsDateBlockContext.Make(pwVariant.ParentMasterVariant, context.PwShop.CurrencyId);                
                var unitCogs = cogsService.CalculateUnitCogs(cogsContexts, translatedLineItem);

                _pushLogger.Debug(
                    "Computed CoGS for new Order Line Item: " + 
                    $"{translatedLineItem.ShopifyOrderLineId}  - {unitCogs}");
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

