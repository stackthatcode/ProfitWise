using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Utility;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.Steps
{
    public class OrderRefreshStep
    {
        private readonly IPushLogger _pushLogger;
        private readonly ShopifyOrderDiagnosticShim _diagnostic;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _refreshServiceConfiguration;
        private readonly PwShopRepository _shopRepository;




        public OrderRefreshStep(
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration refreshServiceConfiguration,
                PwShopRepository shopRepository,
                IPushLogger logger,
                ShopifyOrderDiagnosticShim diagnostic)

        {
            _pushLogger = logger;
            _diagnostic = diagnostic;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _refreshServiceConfiguration = refreshServiceConfiguration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Load Batch State
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var batchState = batchStateRepository.Retrieve();

            // Get User Preferences
            var preferencesRepository = _multitenantFactory.MakePreferencesRepository(shop);
            var preferences = preferencesRepository.Retrieve();

            // CASE #1 - first time loading - don't do any additional updates
            if (batchState.OrderDatasetStart == null)
            {
                FirstTimeLoadWorker(shopCredentials, shop, preferences);
                return;
            }

            // CASE #2 - Order Dataset Start Date has moved back in time
            if (preferences.StartingDateForOrders < batchState.OrderDatasetStart )
            {
                EarlierStartDateWorker(shopCredentials, shop, preferences);
            }

            // CASE #3 - update to get the latest Orders since last update
            RoutineRefreshWorker(shopCredentials, shop);
        }

        private void RoutineRefreshWorker(ShopifyCredentials shopCredentials, PwShop shop)
        {
            _pushLogger.Info($"Routine Order refresh for {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            var updatefilter = 
                new OrderFilter()
                {
                    UpdatedAtMin = preBatchState.OrderDatasetEnd.Value.AddMinutes(-15)
                }
                .OrderByUpdateAtAscending();

            RefreshOrders(updatefilter, shopCredentials, shop);

            // Update Batch State End to now-ish
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetEnd = DateTime.Now.AddMinutes(-15); // Fudge factor for clock disparities
            batchStateRepository.Update(postBatchState);

            _pushLogger.Info("Complete: " + preBatchState.ToString());
        }

        private void EarlierStartDateWorker(ShopifyCredentials shopCredentials, PwShop shop, PwPreferences preferences)
        {
            _pushLogger.Info($"Expanding Order date range for {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            var filter = new OrderFilter()
                {
                    CreatedAtMin = preferences.StartingDateForOrders,
                    CreatedAtMax = preBatchState.OrderDatasetStart,
                }
                .OrderByCreatedAtDescending();


            RefreshOrders(filter, shopCredentials, shop);


            // When updating Batch State, simply move Start to Preferences' new Start
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = preferences.StartingDateForOrders;
            batchStateRepository.Update(postBatchState);
            _pushLogger.Info("Complete: " + postBatchState);
        }

        private void FirstTimeLoadWorker(ShopifyCredentials shopCredentials, PwShop shop, PwPreferences preferences)
        {
            _pushLogger.Info($"Loading Orders first time for Shop {shop.PwShopId}");

            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var preBatchState = batchStateRepository.Retrieve();

            var filter = new OrderFilter()
                {
                    CreatedAtMin = preferences.StartingDateForOrders
                }
                .OrderByCreatedAtDescending();

            var end = DateTime.Now.AddMinutes(-15); // Fudge factor for clock disparities
            preBatchState.OrderDatasetEnd = end;
            

            RefreshOrders(filter, shopCredentials, shop);

            // Update Post Batch State so Start Date equals that which is in Preferences
            var postBatchState = batchStateRepository.Retrieve();
            postBatchState.OrderDatasetStart = preferences.StartingDateForOrders;
            batchStateRepository.Update(postBatchState);
            _pushLogger.Info("Complete: " + postBatchState);
        }



        // All the "worker" functions call Refresh Orders main function
        private void RefreshOrders(
                OrderFilter filter, ShopifyCredentials shopCredentials, PwShop shop)
        {
            var orderApiRepository = _apiRepositoryFactory.MakeOrderApiRepository(shopCredentials);
            var productRepository = _multitenantFactory.MakeProductRepository(shop);
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantRepository.RetrieveAllMasterVariants();
            masterProductCatalog.LoadMasterVariants(masterVariants);

            var context = new OrderRefreshContext
            {
                ShopifyShop = shop,
                Products = masterProductCatalog
            };

            var count = orderApiRepository.RetrieveCount(filter);
            _pushLogger.Info($"{count} Orders to process ({filter})");

            var numberofpages =
                PagingFunctions.NumberOfPages(
                    _refreshServiceConfiguration.MaxOrderRate, count);

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"Page {pagenumber} of {numberofpages} pages");

                var importedOrders = orderApiRepository.Retrieve(filter, pagenumber, _refreshServiceConfiguration.MaxOrderRate);
                WriteOrdersToPersistence(importedOrders, context);

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

        protected virtual void WriteOrdersToPersistence(IList<Order> importedOrders, OrderRefreshContext context)
        {
            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.ShopifyShop);

            _pushLogger.Info($"{importedOrders.Count} Orders to process");

            using (var trans = orderRepository.InitiateTransaction())
            {
                var orderIdList = importedOrders.Select(x => x.Id).ToList();

                // A filtered list of Existing Orders and Line Items for possible update
                var existingShopifyOrders = orderRepository.RetrieveOrders(orderIdList);
                var existingShopifyOrderLineItems = orderRepository.RetrieveOrderLineItems(orderIdList);
                existingShopifyOrders.AppendLineItems(existingShopifyOrderLineItems);
                context.CurrentExistingOrders = existingShopifyOrders;

                foreach (var importedOrder in importedOrders)
                {
                    WriteOrderToPersistence(importedOrder, context);
                }

                trans.Commit();
            }
        }



        private void WriteOrderToPersistence(Order importedOrder, OrderRefreshContext context)
        {
            if (_diagnostic.ShopId == context.ShopifyShop.PwShopId &&
                _diagnostic.OrderIds.Contains(importedOrder.Id))
            {
                _pushLogger.Debug(importedOrder.ToString());
            }
            
            var existingOrder = 
                context.CurrentExistingOrders
                    .FirstOrDefault(x => x.ShopifyOrderId == importedOrder.Id);

            if (existingOrder == null && importedOrder.Cancelled == true)
            {
                _pushLogger.Debug(
                        $"Skipping cancelled Order: {importedOrder.Name} / {importedOrder.Id} for {importedOrder.Email}");
                return;
            }

            var orderRepository = _multitenantFactory.MakeShopifyOrderRepository(context.ShopifyShop);

            if (existingOrder != null && importedOrder.Cancelled == true)
            {
                _pushLogger.Debug(
                        $"Deleting cancelled Order: {importedOrder.Name} / {importedOrder.Id} for {importedOrder.Email}");

                orderRepository.DeleteOrderLineItems(importedOrder.Id);
                orderRepository.DeleteOrder(importedOrder.Id);
                return;
            }

            if (existingOrder == null)
            {
                _pushLogger.Debug(
                    $"Inserting new Order: {importedOrder.Name} / {importedOrder.Id} for {importedOrder.Email}");

                var translatedOrder = importedOrder.ToShopifyOrder(context.ShopifyShop.PwShopId);
                _pushLogger.Debug($"Translating Shopify Order {importedOrder.Name} ({importedOrder.Id}) to ProfitWise data model");
                orderRepository.InsertOrder(translatedOrder);

                foreach (var translatedLineItem in translatedOrder.LineItems)
                {

                    // Important *** this is where the PW Product Id gets assigned to Order Line Item
                    translatedLineItem.PwProductId = FindOrCreatePwProductId(context, translatedLineItem);

                    _pushLogger.Debug(
                        $"Inserting new Order Line Item: {translatedOrder.OrderNumber} / ShopifyOrderId: {translatedOrder.ShopifyOrderId} / " +
                        $"ShopifyOrderLineId: {translatedLineItem.ShopifyOrderLineId} / PwProductId {translatedLineItem.PwProductId}");

                    orderRepository.InsertOrderLineItem(translatedLineItem);
                }
            }
            else
            {
                _pushLogger.Debug(
                    $"Updating existing Order: {importedOrder.OrderNumber} / {importedOrder.ShopifyOrderId} for {importedOrder.Email}");

                importedOrder.CopyIntoExistingOrderForUpdate(existingOrder);
                orderRepository.UpdateOrder(existingOrder);

                foreach (var importedLineItem in importedOrder.LineItems)
                {
                    var existingLineItem =
                        existingShopifyOrderLineItems.FirstOrDefault(
                            x => x.ShopifyOrderId == importedLineItem.ShopifyOrderId &&
                                x.ShopifyOrderLineId == importedLineItem.ShopifyOrderLineId);

                    _pushLogger.Debug(
                            $"Updating existing Order Line Item: {importedOrder.OrderNumber} / " +
                            $"{importedOrder.ShopifyOrderId} / {importedLineItem.ShopifyOrderLineId}");

                    existingLineItem.TotalRestockedQuantity = importedLineItem.TotalRestockedQuantity;
                    existingLineItem.GrossRevenue = importedLineItem.GrossRevenue;

                    // IF Shopify Product or Variant was deleted, we'll see it here from Shopify
                    existingLineItem.ShopifyProductId = importedLineItem.ShopifyProductId;
                    existingLineItem.ShopifyVariantId = importedLineItem.ShopifyVariantId;
                    orderRepository.UpdateOrderLineItem(existingLineItem);
                }
            }            
        }

        public long FindOrCreatePwProductId(OrderRefreshContext context, ShopifyOrderLineItem importedLineItem)
        {
            // *** IMPORTANT attempt to assign a PW Product with exact match on Shopify Product Id and Variant Id
            var shopifyVariant = context.ShopifyVariants.FirstOrDefault(x => x.ExactMatchToLineItem(importedLineItem));

            if (shopifyVariant != null)
            {
                return shopifyVariant.PwProductId.Value;
            }

            // Match by SKU / Title
            var pwproduct =  context.PwProducts.FirstOrDefault(x => x.Sku == importedLineItem.Sku &&
                                                           x.Name == importedLineItem.Name);

            if (pwproduct != null)
            {
                return pwproduct.PwProductId;
            }

            // Provision new PW Product => return the new PWProductId
            var repository = _multitenantFactory.MakeProductRepository(context.ShopifyShop);

            var pwProduct = new PwProduct()
            {
                ShopId =  context.ShopifyShop.PwShopId,
                ProductTitle = importedLineItem.ProductTitle,
                VariantTitle = importedLineItem.VariantTitle,
                Name = importedLineItem.Name,
                Sku = importedLineItem.Sku,
                Tags = importedLineItem.ParentOrder.Tags,
                Price = importedLineItem.UnitPrice,
                Inventory = 0,
            };

            var newPwProductId = repository.InsertProduct(pwProduct);
            pwProduct.PwProductId = newPwProductId;
            context.AddNewPwProduct(pwProduct);

            _pushLogger.Debug(
                $"Provisioned a new PwProduct {newPwProductId} from Order Line Item " +
                $"{importedLineItem.ShopifyOrderId} {importedLineItem.ShopifyOrderLineId}");

            return newPwProductId;
        }
    }
}

