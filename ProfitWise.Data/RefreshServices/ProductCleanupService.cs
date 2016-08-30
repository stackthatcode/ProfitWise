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
using Push.Utilities.Helpers;

namespace ProfitWise.Data.RefreshServices
{
    public class ProductCleanupService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly PwShopRepository _shopRepository;


        public ProductCleanupService(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration configuration,
                PwShopRepository shopRepository)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantRepositoryFactory = multitenantRepositoryFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            DateTime productRefreshStartTime = DateTime.Now;

            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Create an instance of multi-tenant-aware repositories
            var batchStateRepository = _multitenantRepositoryFactory.MakeBatchStateRepository(shop);
            var productRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);
            var variantDataRepository = this._multitenantRepositoryFactory.MakeVariantRepository(shop);

            // Load Batch State
            var batchState = batchStateRepository.Retrieve();

            // Get all existing Variant and ProfitWise Products
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            masterProductCatalog.LoadMasterVariants(masterVariants);
            

            // Import all Product "destroy" Events
            var fromDate = batchState.ProductsLastUpdated ?? productRefreshStartTime.AddMinutes(-15);
            var events = RetrieveAllProductDestroyEvents(shopCredentials, fromDate);

            // Delete all ShopifyVariant mappings ...???
            //DeleteVariantsThatNoLongerLiveInShopify(shop, importedProducts, existingVariants, profitWiseProducts);

            //// Delete all Products flagged for destruction by Shopify Events
            //DeleteProductsFlaggedByShopifyForDeletion(events);

            // Update Batch State
            batchState.ProductsLastUpdated = DateTime.Now.AddMinutes(-15);
            batchStateRepository.Update(batchState);
        }



        public virtual IList<Event> RetrieveAllProductDestroyEvents(ShopifyCredentials shopCredentials, DateTime fromDate)
        {
            var eventApiRepository = _apiRepositoryFactory.MakeEventApiRepository(shopCredentials);
            var filter = new EventFilter()
            {
                CreatedAtMin = fromDate,
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


        //private void DeleteVariantsThatNoLongerLiveInShopify(
        //            PwShop shop, IList<Product> importedProducts,
        //            IList<ShopifyVariant> existingVariants, IList<PwProduct> profitWiseProducts)
        //{
        //    var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);
            
        //    // Step #1 - Delete Variants that are missing from the latest batch of imported Products
        //    var importedProductIds = importedProducts.Select(x => x.Id).ToList();
        //    var importedVariants = importedProducts.SelectMany(x => x.Variants).ToList();

        //    var variantsWithProductIdsInImportList =
        //        existingVariants.Where(variant => importedProductIds.Contains(variant.ShopifyProductId));

        //    foreach (var existingVariant in variantsWithProductIdsInImportList)
        //    {
        //        if (importedVariants.All(x => x.Id != existingVariant.ShopifyVariantId))
        //        {
        //            var pwProduct = profitWiseProducts.FirstOrDefault(x => x.PwProductId == existingVariant.PwProductId);

        //            _pushLogger.Debug(
        //                $"Deleting Variant: {existingVariant.ShopifyProductId} ({existingVariant.ShopifyVariantId}) for PW Product {pwProduct.Name}");

        //            variantDataRepository.Delete(existingVariant.ShopifyProductId, existingVariant.ShopifyVariantId);
        //        }
        //    }
        //}

        //private void DeleteProductsFlaggedByShopifyForDeletion(IList<Event> events)
        //{
        //    var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);

        //    // Step #2 - Delete Products which trigger a "destroy" Event
        //    foreach (var @event in events)
        //    {
        //        if (@event.Verb == EventVerbs.Destroy && @event.SubjectType == EventTypes.Product)
        //        {
        //            _pushLogger.Debug(
        //               $"Deleting all Variants for Product with Id {@event.SubjectId} (via 'destroy' event)");
        //            variantDataRepository.DeleteByProduct(@event.SubjectId);
        //        }
        //    }

        //}

        //private void WriteProductToDatabase(
        //        PwShop shop, Product importedProduct, IList<ShopifyProduct> existingProducts,
        //        ShopifyProductRepository productDataRepository)
        //{
        //    var existingProduct = existingProducts.FirstOrDefault(x => x.ShopifyProductId == importedProduct.Id);

        //    if (existingProduct == null)
        //    {
        //        var productData = new ShopifyProduct()
        //        {
        //            ShopId = shop.ShopId,
        //            ShopifyProductId = importedProduct.Id,
        //            Title = importedProduct.Title,
        //        };

        //        _pushLogger.Debug(
        //            $"Inserting Product: {importedProduct.Title} ({importedProduct.Id})");
        //        productDataRepository.Insert(productData);
        //    }
        //    else
        //    {
        //        existingProduct.Title = importedProduct.Title;
        //        _pushLogger.Debug(
        //            $"Updating Product: {importedProduct.Title} ({importedProduct.Id})");

        //        productDataRepository.Update(existingProduct);
        //    }
        //}


    }
}

