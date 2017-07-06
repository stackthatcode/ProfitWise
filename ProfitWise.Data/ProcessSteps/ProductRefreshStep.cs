using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductRefreshStep
    {
        private readonly BatchLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly ShopRepository _shopRepository;
        private readonly TimeZoneTranslator _timeZoneTranslator;

        // This is to account for a whole range of possibilities, from clock offsets,
        // ... to the shop's Timezone changing
        public const int MinutesFudgeFactor = -75;
        

        public ProductRefreshStep(
                BatchLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                ShopRepository shopRepository,
                TimeZoneTranslator timeZoneTranslator)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
            _timeZoneTranslator = timeZoneTranslator;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            ExecuteAuxillary(shopCredentials);
        }

        private void ExecuteAuxillary(ShopifyCredentials shopCredentials)
        {
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerUserId);
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            
            // Retrieve Batch State and compute Event parameter
            var batchState = batchStateRepository.Retrieve();
            var processStartTimeUtc = DateTime.UtcNow;
            var fromDateForDestroyUtc = batchState.ProductsLastUpdated ?? processStartTimeUtc;

            // Write Products to our database
            WriteAllProductsFromShopify(shop, shopCredentials, batchState);
            
            // Delete all Products with "destroy" Events
            SetDeletedProductsToInactive(shop, shopCredentials, fromDateForDestroyUtc);

            // Update Batch State
            batchState.ProductsLastUpdated = processStartTimeUtc;
            _pushLogger.Debug($"Updating BatchState -> ProductsLastUpdated to {batchState.ProductsLastUpdated}");
            batchStateRepository.Update(batchState);
        }

        public virtual IList<Product> WriteAllProductsFromShopify(
                PwShop shop, ShopifyCredentials shopCredentials, PwBatchState batchState)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);

            var filter = new ProductFilter();
            if (batchState.ProductsLastUpdated != null)
            {
                filter.UpdatedAtMinUtc = 
                    batchState.ProductsLastUpdated.Value.AddMinutes(MinutesFudgeFactor);
                _pushLogger.Debug($"Retrieving Products Updated after {filter.UpdatedAtMinUtc}");
            }

            var count = productApiRepository.RetrieveCount(filter);
            _pushLogger.Info($"Executing Refresh for {count} Products");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Product>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages");

                var products = productApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(products);
                WriteProductsToDatabase(shop, products);
            }

            return results;
        }

        private void WriteProductsToDatabase(PwShop shop, IList<Product> importedProducts)
        {
            _pushLogger.Info($"{importedProducts.Count} Products to process from Shopify");

            var retrievalService = _multitenantFactory.MakeCatalogRetrievalService(shop);
            var builderService = _multitenantFactory.MakeCatalogBuilderService(shop);
            var existingMasterProducts = retrievalService.RetrieveFullCatalog();

            foreach (var importedProduct in importedProducts)
            {
                using (var transaction = retrievalService.InitiateTransaction())
                {
                    var productBuildContext = new ProductBuildContext(importedProduct, existingMasterProducts);

                    // First save/update the Product and Master Product
                    var product = WriteProductToDatabase(shop, productBuildContext);
                    
                    // Next save/update the Variant and Master Variant
                    foreach (var importedVariant in importedProduct.Variants)
                    {
                        var variantBuildContext = new VariantBuildContext(importedVariant, existingMasterProducts, product);
                        WriteVariantToDatabase(shop, variantBuildContext);
                    }

                    // Finally, mark Variants that weren't in the import as Inactive
                    var activeVariantIds = importedProduct.Variants.Select(x => x.Id).ToList();
                    builderService.FlagMissingVariantsAsInactive(
                            existingMasterProducts, product.ShopifyProductId, activeVariantIds);
                    transaction.Commit();
                }
            }
        }

        public PwProduct WriteProductToDatabase(PwShop shop, ProductBuildContext context)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);

            // Find or create the Master Product
            var masterProduct = context.ExistingMasterProducts.FindMasterProduct(context);
            if (masterProduct == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Product for Title: {context.Title} " + 
                    $"and Vendor: {context.Vendor}");

                masterProduct = service.CreateMasterProduct();
                context.ExistingMasterProducts.Add(masterProduct);
            }

            // Find or create the Product
            var product = masterProduct.FindProduct(context);
            if (product == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Product for Title: {context.Title} " +
                    $"and Vendor: {context.Vendor} " + 
                    $"and ShopifyProductId: {context.ShopifyProductId}");

                product = service.CreateProductAndAssignToMaster(context, masterProduct);
                service.UpdateActiveProduct(context);
            }
            else
            {
                service.UpdateProduct(context, product);
            }
            return product;
        }

        public void WriteVariantToDatabase(PwShop shop, VariantBuildContext context)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);

            var masterVariant = context.MasterProduct.FindMasterVariant(context);
            if (masterVariant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Variant for Title: {context.Title} and Sku: {context.Sku}");

                var newMasterVariant = service.CreateAndAssignMasterVariant(context);
                context.TargetMasterVariant = newMasterVariant;
            }
            else
            {
                context.TargetMasterVariant = masterVariant;
            }

            var variant = context.TargetMasterVariant.FindVariant(context);
            if (variant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Variant for Title: {context.Title} " +
                    $"and Sku: {context.Sku} and Shopify Variant Id: {context.ShopifyVariantId}");

                variant = service.CreateVariant(context);                
                service.UpdateActiveVariant(context.AllMasterProducts, context.ShopifyVariantId);
            }

            var inventory = context.InventoryTracked ? context.Inventory : (int?) null;

            _pushLogger.Debug(
                $"Updating Variant {variant.PwVariantId} price range: " +
                $"{context.Price} to {context.Price} and setting inventory to {inventory}");

            variantRepository.UpdateVariant(
                variant.PwVariantId, context.Price, context.Price, variant.Sku, 
                inventory, DateTime.UtcNow);
        }

        private void SetDeletedProductsToInactive(
                PwShop shop, ShopifyCredentials shopCredentials, DateTime fromDateUtc)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);

            var catalogRetrievalService = this._multitenantFactory.MakeCatalogRetrievalService(shop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(shop);

            var masterProducts = catalogRetrievalService.RetrieveFullCatalog();
            var events = RetrieveAllProductDestroyEvents(shopCredentials, shop, fromDateUtc);

            foreach (var destoryEvent in events)
            {
                if (destoryEvent.Verb == EventVerbs.Destroy && destoryEvent.SubjectType == EventTypes.Product)
                {
                    var shopifyProductId = destoryEvent.SubjectId;
                    _pushLogger.Debug($"Marking all Products with Shopify Id {shopifyProductId} to Inactive (via 'destroy' event)");

                    // Optimized SQL update of IsActive status
                    productRepository.UpdateProductIsActiveByShopifyId(shopifyProductId, false);
                    
                    // Next, using in-memory catalog, update Primary
                    foreach (var masterProduct in masterProducts.FindMasterProductsByShopifyId(shopifyProductId))
                    {
                        var products = masterProduct.FindProductsByShopifyId(shopifyProductId);                        
                        products.ForEach(x => x.IsActive = false);
                        
                        catalogService.AutoUpdateAndSavePrimary(masterProduct);
                    }
                }
            }
        }
        
        private IList<Event> RetrieveAllProductDestroyEvents(
                ShopifyCredentials shopCredentials, PwShop shop, DateTime fromDateUtc)
        {
            var eventApiRepository = _apiRepositoryFactory.MakeEventApiRepository(shopCredentials);

            var fromDateShopTz = _timeZoneTranslator.FromUtcToShopTz(fromDateUtc, shop.TimeZone);
            var filter = new EventFilter
            {
                CreatedAtMinUtc = fromDateShopTz.AddMinutes(MinutesFudgeFactor),
                Verb = EventVerbs.Destroy,
                Filter = EventTypes.Product
            };

            _pushLogger.Debug($"Retrieve all Product Destroy Events from {filter.CreatedAtMinUtc}");
            var count = eventApiRepository.RetrieveCount(filter);

            _pushLogger.Info($"Executing Refresh for {count} Product 'destroy' Events");
            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Event>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages");
                var events = eventApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(events);
            }

            return results;
        }

    }
}

