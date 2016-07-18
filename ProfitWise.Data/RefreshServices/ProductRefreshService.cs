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
    public class ProductRefreshService
    {
        private readonly IPushLogger _pushLogger;
        private readonly ApiRepositoryFactory _apiRepositoryFactory;
        private readonly MultitenantRepositoryFactory _multitenantRepositoryFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly ShopRepository _shopRepository;


        public ProductRefreshService(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantRepositoryFactory multitenantRepositoryFactory,
                RefreshServiceConfiguration configuration,
                ShopRepository shopRepository)
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
            var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);
            var profitWiseProductRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);

            // Load Batch State
            var batchState = batchStateRepository.Retrieve();

            // Import Products from Shopify
            var importedProducts = RetrieveAllProducts(shopCredentials, batchState);

            // Get all existing Variant and ProfitWise Products
            var existingVariants = variantDataRepository.RetrieveAll();
            var profitWiseProducts = profitWiseProductRepository.RetrieveAll();

            // Write Products to our database
            WriteAllProductsToDatabase(shop, importedProducts, existingVariants, profitWiseProducts);

            // Import all Product "destroy" Events
            var fromDate = batchState.ProductsLastUpdated ?? productRefreshStartTime.AddMinutes(-15);
            var events = RetrieveAllProductDestroyEvents(shopCredentials, fromDate);

            // Delete all Products flagged for destruction
            DeleteProducts(shop, events, importedProducts, existingVariants, profitWiseProducts);

            // Update Batch State
            batchState.ProductsLastUpdated = DateTime.Now.AddMinutes(-15);
            batchStateRepository.Update(batchState);
        }

        private void WriteAllProductsToDatabase(ShopifyShop shop, IList<Product> importedProducts, IList<ShopifyVariant> existingVariants, IList<PwProduct> profitWiseProducts)
        {
            _pushLogger.Info($"{importedProducts.Count} Products to process");
            var importedVariants = importedProducts.SelectMany(x => x.Variants).ToList();
            foreach (var variant in importedVariants)
            {
                WriteVariantToDatabase(shop, variant, existingVariants, profitWiseProducts);
            }
        }

        private void WriteVariantToDatabase(
                    ShopifyShop shop, Variant importedVariant,
                    IList<ShopifyVariant> existingVariants, IList<PwProduct> profitWiseProducts)
        {
            var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);
            var profitWiseProductRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);

            var existingVariant =
                existingVariants.FirstOrDefault(x =>
                        x.ShopifyProductId == importedVariant.ParentProduct.Id &&
                        x.ShopifyVariantId == importedVariant.Id);

            if (existingVariant == null)
            {
                _pushLogger.Debug($"Inserting Variant: {importedVariant.Title} ({importedVariant.Id}) for Product {importedVariant.ParentProduct.Title} ({importedVariant.ParentProduct.Id})");

                var profitWiseProduct = new PwProduct
                {
                    ShopId = shop.ShopId,
                    ProductTitle = importedVariant.ParentProduct.Title,
                    VariantTitle = importedVariant.Title,
                    Name = importedVariant.ParentProduct.Title + " - " + importedVariant.Title,
                    Sku = importedVariant.Sku,
                    Price = importedVariant.Price,
                    Inventory = importedVariant.Inventory,
                    Tags = importedVariant.ParentProduct.Tags,
                };

                var newPwProductId = profitWiseProductRepository.Insert(profitWiseProduct);

                var variantData = new ShopifyVariant()
                {
                    ShopId = shop.ShopId,
                    ShopifyVariantId = importedVariant.Id,
                    ShopifyProductId = importedVariant.ParentProduct.Id,
                    PwProductId = newPwProductId,
                };

                variantDataRepository.Insert(variantData);
            }
            else
            {
                _pushLogger.Debug($"Updating Variant: {importedVariant.Title} ({importedVariant.Id}) for Product {importedVariant.ParentProduct.Title} ({importedVariant.ParentProduct.Id})");

                var profitWiseProduct =
                    profitWiseProducts.FirstOrDefault(x => x.PwProductId == existingVariant.PwProductId);

                // NOTE: go ahead and throw an error if this doesn't exist - inconsistent state that needs to be resolved
                profitWiseProduct.ProductTitle = importedVariant.ParentProduct.Title;
                profitWiseProduct.VariantTitle = importedVariant.Title;
                profitWiseProduct.Name = importedVariant.ParentProduct.Title + " - " + importedVariant.Title;
                profitWiseProduct.Sku = importedVariant.Sku;
                profitWiseProduct.Price = importedVariant.Price;
                profitWiseProduct.Inventory = importedVariant.Inventory;
                profitWiseProduct.Tags = importedVariant.ParentProduct.Tags;

                profitWiseProductRepository.Update(profitWiseProduct);
            }
        }


        public virtual IList<Product> RetrieveAllProducts(ShopifyCredentials shopCredentials, PwBatchState batchState)
        {
            var productApiRepository = _apiRepositoryFactory.MakeProductApiRepository(shopCredentials);
            var filter = new ProductFilter()
            {
                UpdatedAtMin = batchState.ProductsLastUpdated,
            };
            var count = productApiRepository.RetrieveCount(filter);

            _pushLogger.Info($"Executing Refresh for {count} Products");

            var numberofpages = PagingFunctions.NumberOfPages(_configuration.MaxProductRate, count);
            var results = new List<Product>();

            for (int pagenumber = 1; pagenumber <= numberofpages; pagenumber++)
            {
                _pushLogger.Info(
                    $"Page {pagenumber} of {numberofpages} pages");

                var products = productApiRepository.Retrieve(filter, pagenumber, _configuration.MaxProductRate);
                results.AddRange(products);
            }

            return results;
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


        private void DeleteProducts(
                    ShopifyShop shop, IList<Event> events, IList<Product> importedProducts,
                    IList<ShopifyVariant> existingVariants, IList<PwProduct> profitWiseProducts)
        {
            var variantDataRepository = this._multitenantRepositoryFactory.MakeShopifyVariantRepository(shop);
            
            // Step #1 - Delete Variants that are missing from the latest batch of imported Products
            var importedProductIds = importedProducts.Select(x => x.Id).ToList();
            var importedVariants = importedProducts.SelectMany(x => x.Variants).ToList();

            foreach (var existingVariant in existingVariants.Where(variant => importedProductIds.Contains(variant.ShopifyProductId)))
            {
                if (importedVariants.All(x => x.Id != existingVariant.ShopifyVariantId))
                {
                    var pwProduct = profitWiseProducts.FirstOrDefault(x => x.PwProductId == existingVariant.PwProductId);

                    _pushLogger.Debug(
                        $"Deleting Variant: {existingVariant.ShopifyProductId} ({existingVariant.ShopifyVariantId}) for PW Product {pwProduct.Name}");

                    variantDataRepository.Delete(existingVariant.ShopifyProductId, existingVariant.ShopifyVariantId);
                }
            }

            // Step #2 - Delete Products which trigger a "destroy" Event
            foreach (var @event in events)
            {
                if (@event.Verb == EventVerbs.Destroy && @event.SubjectType == EventTypes.Product)
                {
                    _pushLogger.Debug(
                       $"Deleting all Variants for Product with Id {@event.SubjectId} (via 'destroy' event)");
                    variantDataRepository.DeleteByProduct(@event.SubjectId);
                }
            }
        }





        private void WriteProductToDatabase(
                ShopifyShop shop, Product importedProduct, IList<ShopifyProduct> existingProducts,
                ShopifyProductRepository productDataRepository)
        {
            var existingProduct = existingProducts.FirstOrDefault(x => x.ShopifyProductId == importedProduct.Id);

            if (existingProduct == null)
            {
                var productData = new ShopifyProduct()
                {
                    ShopId = shop.ShopId,
                    ShopifyProductId = importedProduct.Id,
                    Title = importedProduct.Title,
                };

                _pushLogger.Debug(
                    $"Inserting Product: {importedProduct.Title} ({importedProduct.Id})");
                productDataRepository.Insert(productData);
            }
            else
            {
                existingProduct.Title = importedProduct.Title;
                _pushLogger.Debug(
                    $"Updating Product: {importedProduct.Title} ({importedProduct.Id})");

                productDataRepository.Update(existingProduct);
            }
        }


    }
}

