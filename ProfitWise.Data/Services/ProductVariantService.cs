using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Steps;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class ProductVariantService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        public PwShop PwShop { get; set; }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;


        public ProductVariantService(
                    IPushLogger logger,
                    MultitenantFactory multitenantFactory)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
        }
        

        public PwMasterProduct FindOrCreateNewMasterProduct
                            (IList<PwMasterProduct> masterProducts, Product importedProduct)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

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
                    PwShopId = this.PwShop.PwShopId,
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

        public PwProduct FindOrCreateNewProduct(
                    PwMasterProduct masterProduct, Product importedProduct)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

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
                    PwShopId = PwShop.PwShopId,
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


        public PwMasterVariant FindOrCreateNewMasterVariant(
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
                
                var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);

                var masterVariant = new PwMasterVariant()
                {
                    PwShopId = shop.PwShopId,
                    ParentProduct = product,
                    Exclude = false,
                    StockedDirectly = StockedDirectlyDefault,
                    PwProductId = product.PwProductId,
                    Variants = new List<PwVariant>(),
                };

                masterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(masterVariant);

                var newVariant = new PwVariant()
                {
                    PwShopId = shop.PwShopId,
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

    }
}

