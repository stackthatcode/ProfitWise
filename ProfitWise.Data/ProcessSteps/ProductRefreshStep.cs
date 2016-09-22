using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductRefreshStep
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly PwShopRepository _shopRepository;
        private readonly TimeZoneTranslator _timeZoneTranslator;


        // 60 minutes to account for daylight savings + 15 minutes to account for clock inaccuracies
        public const int MinutesFudgeFactor = 75;


        public ProductRefreshStep(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                PwShopRepository shopRepository,
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
            DateTime processStepStartTime = DateTime.Now;

            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerUserId);
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var service = this._multitenantFactory.MakeCatalogBuilderService(shop);

            // Load Batch State
            var batchState = batchStateRepository.Retrieve();

            // Retrieve existing catalog from ProfitWise
            var masterProducts = service.RetrieveFullCatalog();

            // Write Products to our database
            WriteAllProductsFromShopify(shop, masterProducts, shopCredentials, batchState);

            // Delete all Products with "destroy" Events
            // If this is the first time, we'll use the start time of this Refresh Step (minus a fudge factor)
            var fromDateForDestroy = batchState.ProductsLastUpdated ?? processStepStartTime.AddMinutes(-15);
            SetProductsDeletedByShopifyToInactive(shop, masterProducts, shopCredentials, fromDateForDestroy);

            // Update Batch State
            batchState.ProductsLastUpdated = DateTime.Now.AddMinutes(-15);
            batchStateRepository.Update(batchState);
        }

        public virtual IList<Product> WriteAllProductsFromShopify(
                PwShop shop, IList<PwMasterProduct> masterProducts, 
                ShopifyCredentials shopCredentials, PwBatchState batchState)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);
            var filter = new ProductFilter();

            if (batchState.ProductsLastUpdated != null)
            {
                var lastUpdatedInShopifyTime =
                    _timeZoneTranslator.TranslateToTimeZone(batchState.ProductsLastUpdated.Value, shop.TimeZone)
                        .AddMinutes(-MinutesFudgeFactor);

                filter.UpdatedAtMin = lastUpdatedInShopifyTime;
            };

            var count = productApiRepository.RetrieveCount(filter);

            _pushLogger.Info($"Executing Refresh for {count} Products");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Product>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info($"Page {pagenumber} of {numberofpages} pages");

                var products = productApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(products);

                WriteProductsToDatabase(shop, products, masterProducts);
            }

            return results;
        }

        private void WriteProductsToDatabase(
                    PwShop shop, IList<Product> importedProducts, IList<PwMasterProduct> masterProducts)
        {
            _pushLogger.Info($"{importedProducts.Count} Products to process from Shopify");

            var repository = _multitenantFactory.MakeProductRepository(shop);

            using (var transaction = repository.InitiateTransaction())
            {
                foreach (var importedProduct in importedProducts)
                {
                    var product = WriteProductToDatabase(shop, masterProducts, importedProduct);
                    var masterProduct = product.ParentMasterProduct;

                    foreach (var importedVariant in importedProduct.Variants)
                    {
                        WriteVariantToDatabase(shop, masterProducts, masterProduct, importedVariant, product,
                            importedProduct);
                    }

                    FlagMissingVariantsAsInactive(shop, masterProducts, importedProduct);
                }

                transaction.Commit();
            }
        }

        private PwProduct WriteProductToDatabase(
                PwShop shop, IList<PwMasterProduct> masterProducts, Product importedProduct)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);

            var masterProduct =
                masterProducts.FindMasterProduct(importedProduct.Title, importedProduct.Vendor);

            if (masterProduct == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Product for Title: {importedProduct.Title} " +
                    $"and Vendor: {importedProduct.Vendor}");

                masterProduct = service.BuildAndSaveMasterProduct();

                if (!masterProducts.Contains(masterProduct))
                {
                    masterProducts.Add(masterProduct);
                }
            }

            var product = masterProduct.FindProduct(
                importedProduct.Title, importedProduct.Vendor, importedProduct.Id);

            if (product == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Product for Title: {importedProduct.Title} " +
                    $"and Vendor: {importedProduct.Vendor} and Shopify Id: {importedProduct.Id}");

                product =
                    service.BuildAndSaveProduct(
                        masterProduct, true, importedProduct.Title, importedProduct.Id, importedProduct.Vendor,
                        importedProduct.Tags, importedProduct.ProductType);
                
                service.UpdateActiveShopifyProduct(masterProducts, product.ShopifyProductId);
            }
            else
            {
                service.UpdateExistingProduct(product, importedProduct.Tags, importedProduct.ProductType);
            }
            return product;
        }

        private void WriteVariantToDatabase(
                PwShop shop, IList<PwMasterProduct> masterProducts, PwMasterProduct masterProduct, 
                Variant importedVariant, PwProduct product, Product importedProduct)
        {
            var service = _multitenantFactory.MakeCatalogBuilderService(shop);
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);

            var masterVariant =
                masterProduct.FindMasterVariant(importedVariant.Sku, importedVariant.Title);

            if (masterVariant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Master Variant for Title: {importedVariant.Title} " +
                    $"and Sku: {importedVariant.Sku}");

                masterVariant =
                    service.BuildAndSaveMasterVariant(
                        product, importedVariant.Title, importedProduct.Id, importedVariant.Id,
                        importedVariant.Sku);
                masterProduct.MasterVariants.Add(masterVariant);
            }

            var variant = masterVariant.FindVariant(importedVariant.Sku, importedVariant.Title, importedVariant.Id);

            if (variant == null)
            {
                _pushLogger.Debug(
                    $"Unable to find Variant for Title: {importedVariant.Title} " +
                    $"and Sku: {importedVariant.Sku} and Shopify Variant Id: {importedVariant.Id}");
                variant =
                    service.BuildAndSaveVariant(masterVariant, true, product, importedVariant.Title,
                        importedVariant.Id, importedVariant.Sku);

                service.UpdateActiveShopifyVariant(masterProducts, variant.ShopifyVariantId);
            }

            var inventory = importedVariant.InventoryTracked ? importedVariant.Inventory : (int?) null;

            _pushLogger.Debug($"Updating Variant {variant.PwVariantId} price range: " +
                              $"{importedVariant.Price} to {importedVariant.Price} and setting inventory to {inventory}");

            variantRepository
                .UpdateVariantPriceAndInventory(
                    variant.PwVariantId, importedVariant.Price, importedVariant.Price, inventory);
        }

        public virtual IList<Event> RetrieveAllProductDestroyEvents(
                                ShopifyCredentials shopCredentials, PwShop shop, DateTime fromDate)
        {
            var eventApiRepository = _apiRepositoryFactory.MakeEventApiRepository(shopCredentials);

            var fromDateInShopify = 
                _timeZoneTranslator.TranslateToTimeZone(fromDate, shop.TimeZone)
                                .AddMinutes(-MinutesFudgeFactor);

            var filter = new EventFilter()
            {
                CreatedAtMin = fromDateInShopify,
                Verb = EventVerbs.Destroy,
                Filter = EventTypes.Product
            };

            var count = eventApiRepository.RetrieveCount(filter);

            _pushLogger.Info($"Executing Refresh for {count} Product 'destroy' Events");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Event>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"Page {pagenumber} of {numberofpages} pages");

                var events = eventApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(events);
            }

            return results;
        }

        private void SetProductsDeletedByShopifyToInactive(
                    PwShop shop, IList<PwMasterProduct> masterProducts, ShopifyCredentials shopCredentials, DateTime fromDateForDestroy)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(shop);

            var events = RetrieveAllProductDestroyEvents(shopCredentials, shop, fromDateForDestroy);

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
                        catalogService.UpdatePrimaryProduct(masterProduct);
                    }
                }
            }
        }

        private void FlagMissingVariantsAsInactive(
                PwShop shop, IList<PwMasterProduct> masterProducts, Product importedProduct)
        {
            // Mark all Variants as InActive that aren't in the import
            var variantRepository = _multitenantFactory.MakeVariantRepository(shop);
            var catalogService = _multitenantFactory.MakeCatalogBuilderService(shop);

            var shopifyProductId = importedProduct.Id;
            var activeShopifyVariantIds = importedProduct.Variants.Select(x => x.Id);

            // Extract
            var allExistingVariants =
                masterProducts
                    .SelectMany(x => x.MasterVariants)
                    .SelectMany(x => x.Variants)
                    .Where(x => x.ShopifyProductId == shopifyProductId);

            var missingFromActive =
                allExistingVariants
                    .Where(x => x.ShopifyVariantId != null)
                    .Where(x => activeShopifyVariantIds.All(activeId => activeId != x.ShopifyVariantId))
                    .ToList();

            missingFromActive.ForEach(variant =>
            {
                variant.IsActive = false;
                _pushLogger.Debug($"Flagging PwVariantId {variant.PwVariantId} as Inactive");
                variantRepository.UpdateVariantIsActive(variant);
                catalogService.UpdatePrimaryVariant(variant.ParentMasterVariant);
            });
        }
    }
}

