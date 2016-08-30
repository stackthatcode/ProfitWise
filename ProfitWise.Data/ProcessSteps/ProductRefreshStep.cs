using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
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


        public ProductRefreshStep(
                IPushLogger logger,
                ApiRepositoryFactory apiRepositoryFactory,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                PwShopRepository shopRepository)
        {
            _pushLogger = logger;
            _apiRepositoryFactory = apiRepositoryFactory;
            _multitenantFactory = multitenantFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Create an instance of multi-tenant-aware repositories
            var batchStateRepository = _multitenantFactory.MakeBatchStateRepository(shop);
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(shop);

            // Load Batch State
            var batchState = batchStateRepository.Retrieve();

            // Import Products from Shopify
            var importedProducts = RetrieveAllProductsFromShopify(shopCredentials, batchState);


            // Get all existing Variant and ProfitWise Products
            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            masterProductCatalog.LoadMasterVariants(masterVariants);


            // Write Products to our database
            WriteAllProductsToDatabase(shop, importedProducts, masterProductCatalog);
            

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
                var service = _multitenantFactory.MakeProductVariantService(shop);

                // Process for creating ProfitWise Master Product, Product, Master Variant & Variant 
                // ... from Shopify Product catalog item
                var masterProduct = 
                    service.FindOrCreateNewMasterProduct(
                        masterProducts, importedProduct.Title, importedProduct.Id);
                
                var product = 
                    service.FindOrCreateNewProduct(
                        masterProduct, importedProduct.Title, importedProduct.Id, importedProduct.Vendor, importedProduct.Tags, importedProduct.ProductType);

                foreach (var importedVariant in importedProduct.Variants)
                {
                    var masterVariant = 
                        service.FindOrCreateNewMasterVariant(
                            product, importedVariant.Title, importedVariant.Id, importedVariant.Sku);
                }
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

    }
}

