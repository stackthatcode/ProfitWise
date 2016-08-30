using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
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
        private readonly PwShopRepository _shopRepository;


        public ProductRefreshService(
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

            // Import Products from Shopify
            var importedProducts = RetrieveAllProductsFromShopify(shopCredentials, batchState);


            // Get all existing Variant and ProfitWise Products
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            masterProductCatalog.LoadMasterVariants(masterVariants);

            // Write Products to our database
            WriteAllProductsToDatabase(shop, importedProducts, masterProductCatalog);
            

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

        private void WriteAllProductsToDatabase(
                    PwShop shop, IList<Product> importedProducts, IList<PwMasterProduct> masterProducts)
        {
            _pushLogger.Info($"{importedProducts.Count} Products to process from Shopify");

            foreach (var importedProduct in importedProducts)
            {
                // ProfitWise Master Product and Product
                var masterProduct = FindMasterProductOrCreateNewIfNecessary(shop, masterProducts, importedProduct);
                var product = FindProductMatchOrCreateNewIfNecessary(shop, masterProduct, importedProduct);

                foreach (var importedVariant in importedProduct.Variants)
                {
                    var masterVariant = FindMasterVariantOrCreateNewIfNecessary(shop, product, importedVariant);
                }
            }
        }


        private PwMasterProduct FindMasterProductOrCreateNewIfNecessary
                            (PwShop shop, IList<PwMasterProduct> masterProducts, Product importedProduct)
        {
            var productRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);

            PwProduct productMatchByTitle =
                masterProducts
                    .SelectMany(x => x.Products)
                    .FirstOrDefault(x => x.Title == importedProduct.Title);

            // Unable to find a single Product with Product Title, then create a new one
            if (productMatchByTitle == null)
            {
                _pushLogger.Debug(
                    $"Creating new Master Product: {importedProduct.Title} (Id = {importedProduct.Id})");

                var masterProduct = new PwMasterProduct()
                {
                    PwShopId = shop.ShopId,
                    Products = new List<PwProduct>(),
                };
                var masterProductId = productRepository.InsertMasterProduct(masterProduct);
                masterProduct.PwMasterProductId = masterProductId;
                return masterProduct;
            }
            else
            {
                _pushLogger.Debug(
                        $"Found existing Master Product: {importedProduct.Title} (Id = {importedProduct.Id}, " +
                        $"PwMasterProductId = {productMatchByTitle.ParentMasterProduct.PwMasterProductId})");

                return productMatchByTitle.ParentMasterProduct;
            }
        }

        private PwProduct FindProductMatchOrCreateNewIfNecessary(
                PwShop shop, PwMasterProduct masterProduct, Product importedProduct)
        {
            var productRepository = this._multitenantRepositoryFactory.MakeProductRepository(shop);

            if (masterProduct.Products.All(x => x.Title != importedProduct.Title))
            {
                throw new ArgumentException(
                    "None of the Master Product's child Product Titles match your imported Product");
            }

            PwProduct productMatchByVendor =
                masterProduct
                    .Products
                    .FirstOrDefault(x => x.Vendor == importedProduct.Vendor);

            if (productMatchByVendor != null)
            {
                _pushLogger.Debug(
                    $"Found existing Product: {importedProduct.Title} (Id = {importedProduct.Id}, " +
                    $"PwProductId = {productMatchByVendor.PwProductId})");

                productMatchByVendor.ShopifyProductId = importedProduct.Id;
                productMatchByVendor.Tags = importedProduct.Tags;
                productMatchByVendor.ProductType = importedProduct.ProductType;

                productRepository.UpdateProduct(productMatchByVendor);
                return productMatchByVendor;
            }
            else
            {
                _pushLogger.Debug(
                    $"Create new Product: {importedProduct.Title} (Id = {importedProduct.Id})");

                // Step #1 - set all other Products to inactive
                foreach (var product in masterProduct.Products)
                {
                    product.Active = false;
                    product.Primary = false;
                    productRepository.UpdateProduct(product);
                }

                // Step #2 - create the new Product
                PwProduct finalProduct = new PwProduct()
                {
                    PwShopId = shop.ShopId,
                    PwMasterProductId = masterProduct.PwMasterProductId,
                    ShopifyProductId = importedProduct.Id,
                    Title = importedProduct.Title,
                    Vendor = importedProduct.Vendor,
                    ProductType = importedProduct.ProductType,
                    Active = true,
                    Primary = true,
                    Tags = importedProduct.Tags,
                    ParentMasterProduct = masterProduct,
                };

                var productId = productRepository.InsertProduct(finalProduct);
                finalProduct.PwProductId = productId;
                finalProduct.ParentMasterProduct = masterProduct;
                masterProduct.Products.Add(finalProduct);
                return finalProduct;
            }
        }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;

        private PwMasterVariant FindMasterVariantOrCreateNewIfNecessary(
                    PwShop shop, PwProduct product, Variant importedVariant)
        {
            var matchingVariantBySkuAndTitle =
                product.MasterVariants
                    .SelectMany(x => x.Variants)
                    .FirstOrDefault(x => x.Sku == importedVariant.Sku && x.Title == importedVariant.Title);

            if (matchingVariantBySkuAndTitle != null)
            {
                _pushLogger.Debug(
                    $"Found existing Master Variant & Variant: {importedVariant.Title}, {importedVariant.Sku} " +
                    $"(Id = {importedVariant.Id}), PwMasterVariantId = {matchingVariantBySkuAndTitle.PwMasterVariantId}, " + 
                    $"PwVariantId = {matchingVariantBySkuAndTitle.PwVariantId})");

                return matchingVariantBySkuAndTitle.ParentMasterVariant;
            }
            else
            {
                _pushLogger.Debug(
                    $"Creating new Master Variant & Variant: {importedVariant.Title}, {importedVariant.Sku} " +
                    $"(Id = {importedVariant.Id})");
                
                var variantRepository = this._multitenantRepositoryFactory.MakeVariantRepository(shop);

                var masterVariant = new PwMasterVariant()
                {
                    PwShopId = shop.ShopId,
                    ParentProduct = product,
                    Exclude = false,
                    StockedDirectly = StockedDirectlyDefault,
                    PwProductId = product.PwProductId,
                    Variants = new List<PwVariant>(),
                };

                masterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(masterVariant);

                var newVariant = new PwVariant()
                {
                    PwShopId = shop.ShopId,
                    ShopifyVariantId = importedVariant.Id,
                    Title = importedVariant.Title,
                    Sku = importedVariant.Sku,
                    PwMasterVariantId = masterVariant.PwMasterVariantId,
                    Primary = true,
                    Active = true, // Because it's in the live catalog!
                    ParentMasterVariant = masterVariant,
                };

                variantRepository.InsertVariant(newVariant);
                masterVariant.Variants.Add(newVariant);

                return masterVariant;
            }
        }



        public virtual IList<Product> RetrieveAllProductsFromShopify(
                    ShopifyCredentials shopCredentials, PwBatchState batchState)
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

