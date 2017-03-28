using System;
using System.Collections.Generic;
using System.Linq;
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
            var service = _multitenantFactory.MakeCatalogRetrievalService(shop);
            
            // Retrieve Batch State and compute Event parameter
            var batchState = batchStateRepository.Retrieve();
            var processStartTimeUtc = DateTime.UtcNow;
            var fromDateForDestroyUtc = batchState.ProductsLastUpdated ?? processStartTimeUtc;

            // Write Products to our database
            WriteAllProductsFromShopify(shop, shopCredentials, batchState);

            // Retrieve the full catalog against which to update Active Status on
            var masterProducts = service.RetrieveFullCatalog();

            // Delete all Products with "destroy" Events
            SetDeletedProductsToInactive(shop, masterProducts, shopCredentials, fromDateForDestroyUtc);

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

            var service = _multitenantFactory.MakeCatalogRetrievalService(shop);
            var masterProducts = service.RetrieveFullCatalog();

            foreach (var importedProduct in importedProducts)
            {
                using (var transaction = service.InitiateTransaction())
                {
                    var productBuildContext = 
                        importedProduct.ToProductBuildContext(masterProducts, isActive: true);

                    var product = WriteProductToDatabase(shop, productBuildContext);
                
                    foreach (var importedVariant in importedProduct.Variants)
                    {
                        var variantBuildContext =
                            importedVariant.ToVariantBuildContext(
                                isActive: true, allMasterProducts: masterProducts, product: product);
                        WriteVariantToDatabase(shop, variantBuildContext);
                    }

                    FlagMissingVariantsAsInactive(shop, productBuildContext);

                    transaction.Commit();
                }
            }
        }

        private PwProduct WriteProductToDatabase(PwShop shop, ProductBuildContext context)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);
            var masterProduct = context.MasterProducts.FindMasterProduct(context);

            if (masterProduct == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Product for Title: {context.Title} " + 
                    $"and Vendor: {context.Vendor}");

                masterProduct = service.CreateMasterProduct();
                context.MasterProducts.Add(masterProduct);
            }

            var product = masterProduct.FindProduct(context);
            if (product == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Product for Title: {context.Title} " +
                    $"and Vendor: {context.Vendor} and Shopify Id: {context.ShopifyProductId}");

                product = service.CreateProduct(masterProduct, context);
                service.UpdateActiveProduct(context.MasterProducts, product.ShopifyProductId);
            }
            else
            {
                _pushLogger.Debug($"Updating existing Product: {product.Title}");
                product.Tags = context.Tags;
                product.ProductType = context.ProductType;
                product.LastUpdated = DateTime.UtcNow;
                
                var productRepository = this._multitenantFactory.MakeProductRepository(shop);
                productRepository.UpdateProduct(product);
            }
            return product;
        }

        private void WriteVariantToDatabase(PwShop shop, VariantBuildContext context)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);

            var masterVariant = context.MasterProduct.FindMasterVariant(context);
            if (masterVariant == null)
            {
                _pushLogger.Debug($"Unable to find Master Variant for Title: {context.Title} and Sku: {context.Sku}");
                var newMasterVariant = service.CreateMasterVariant(context);

                context.MasterProduct.MasterVariants.Add(newMasterVariant);
                context.MasterVariant = newMasterVariant;
            }
            else
            {
                context.MasterVariant = masterVariant;
            }

            var variant = context.MasterVariant.FindVariant(context);
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
                variant.PwVariantId, context.Price, context.Price, variant.Sku, inventory, DateTime.UtcNow);
        }

        private void SetDeletedProductsToInactive(
                PwShop shop, IList<PwMasterProduct> masterProducts, ShopifyCredentials shopCredentials, 
                DateTime fromDateUtc)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(shop);

            var events = RetrieveAllProductDestroyEvents(shopCredentials, shop, fromDateUtc);
            foreach (var @event in events)
            {
                if (@event.Verb == EventVerbs.Destroy && @event.SubjectType == EventTypes.Product)
                {
                    var shopifyProductId = @event.SubjectId;
                    _pushLogger.Debug(
                        $"Marking all Products with Shopify Id {shopifyProductId} (via 'destroy' event)");

                    productRepository.UpdateProductIsActiveByShopifyId(shopifyProductId, false);

                    foreach (
                        var masterProduct in
                            masterProducts.Where(
                                x => x.Products.Any(product => product.ShopifyProductId == shopifyProductId)))
                    {
                        catalogService.AutoUpdatePrimary(masterProduct);
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

        private void FlagMissingVariantsAsInactive(PwShop shop, ProductBuildContext context)
        {
            // Mark all Variants as InActive that aren't in the import
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);
            var catalogService = _multitenantFactory.MakeCatalogBuilderService(shop);

            // Extract
            var allExistingVariants =
                context.MasterProducts
                    .SelectMany(x => x.MasterVariants)
                    .SelectMany(x => x.Variants)
                    .Where(x => x.ShopifyProductId == context.ShopifyProductId);

            var missingFromActive =
                allExistingVariants
                    .Where(x => x.ShopifyVariantId != null)
                    .Where(x => context.ActiveShopifyVariantIds.All(activeId => activeId != x.ShopifyVariantId))
                    .ToList();

            missingFromActive.ForEach(variant =>
            {
                variant.IsActive = false;
                _pushLogger.Debug($"Flagging PwVariantId {variant.PwVariantId} as Inactive");
                variantRepository.UpdateVariantIsActive(variant);
                catalogService.AutoUpdatePrimary(variant.ParentMasterVariant);
            });
        }
    }
}

