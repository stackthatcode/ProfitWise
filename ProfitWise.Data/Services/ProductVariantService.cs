﻿using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using Push.Foundation.Utilities.Logging;
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
        private const string VariantDefaultTitle = "Default Title";

        public ProductVariantService(
                    IPushLogger logger, MultitenantFactory multitenantFactory)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
        }
        

        public PwMasterProduct FindOrCreateNewMasterProduct
                            (IList<PwMasterProduct> masterProducts, 
                            string title, long shopifyProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

            PwProduct productMatchByTitle =
                masterProducts
                    .SelectMany(x => x.Products)
                    .FirstOrDefault(x => x.Title == title);

            // Unable to find a single Product with Product Title, then create a new one
            if (productMatchByTitle == null)
            {
                _pushLogger.Debug(
                    $"Creating new Master Product: {title} (Id = {shopifyProductId})");

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
                        $"Found existing Master Product: {title} (Id = {shopifyProductId}, " +
                        $"PwMasterProductId = {productMatchByTitle.ParentMasterProduct.PwMasterProductId})");

                return productMatchByTitle.ParentMasterProduct;
            }
        }

        public PwProduct FindOrCreateNewProduct(
                    PwMasterProduct masterProduct, 
                    string title, long shopifyProductId, string vendor, string tags, string productType)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

            //if (masterProduct.Products.All(x => x.Title != title))
            //{
            //    throw new ArgumentException(
            //        "None of the Master Product's child Product Titles match your imported Product");
            //}

            PwProduct productMatchByVendor =
                masterProduct
                    .Products
                    .FirstOrDefault(x => x.Vendor == vendor);

            if (productMatchByVendor != null)
            {
                _pushLogger.Debug(
                    $"Found existing Product: {title} (Id = {shopifyProductId}, " +
                    $"PwProductId = {productMatchByVendor.PwProductId})");

                productMatchByVendor.ShopifyProductId = shopifyProductId;
                productMatchByVendor.Tags = tags;
                productMatchByVendor.ProductType = productType;

                productRepository.UpdateProduct(productMatchByVendor);
                return productMatchByVendor;
            }
            else
            {
                _pushLogger.Debug(
                    $"Create new Product: {title} (Id = {shopifyProductId})");

                // Step #1 - set all other Products to inactive
                foreach (var product in masterProduct.Products)
                {
                    product.IsActive = false;
                    product.IsPrimary = false;
                    productRepository.UpdateProduct(product);
                }

                // Step #2 - create the new Product
                PwProduct finalProduct = new PwProduct()
                {
                    PwShopId = PwShop.PwShopId,
                    PwMasterProductId = masterProduct.PwMasterProductId,
                    ShopifyProductId = shopifyProductId,
                    Title = title,
                    Vendor = vendor,
                    ProductType = productType,
                    IsActive = true,
                    IsPrimary = true,
                    Tags = tags,
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
                    PwProduct product, string title, long shopifyVariantId, string sku)
        {
            var titleSearch = VariantTitleCorrection(title);
            var masterProduct = product.ParentMasterProduct;

            var matchingVariantBySkuAndTitle =
                masterProduct.MasterVariants
                    .SelectMany(x => x.Variants)
                    .FirstOrDefault(x => x.Sku == sku && 
                            VariantTitleCorrection(x.Title) == titleSearch);

            if (matchingVariantBySkuAndTitle != null)
            {
                _pushLogger.Debug(
                    $"Found existing Master Variant & Variant: {title}, {sku} " +
                    $"(Id = {shopifyVariantId}), PwMasterVariantId = {matchingVariantBySkuAndTitle.PwMasterVariantId}, " + 
                    $"PwVariantId = {matchingVariantBySkuAndTitle.PwVariantId})");

                return matchingVariantBySkuAndTitle.ParentMasterVariant;
            }
            else
            {
                _pushLogger.Debug(
                    $"Creating new Master Variant & Variant: {title}, {sku} " +
                    $"(Id = {shopifyVariantId})");
                
                var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);

                var masterVariant = new PwMasterVariant()
                {
                    PwShopId = this.PwShop.PwShopId,
                    PwMasterProductId = masterProduct.PwMasterProductId,
                    ParentMasterProduct = masterProduct,
                    Exclude = false,
                    StockedDirectly = StockedDirectlyDefault,
                    Variants = new List<PwVariant>(),
                };

                masterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(masterVariant);

                var newVariant = new PwVariant()
                {
                    PwShopId = this.PwShop.PwShopId,
                    PwProductId = product.PwProductId,  // This is a permanent association :-)
                    PwMasterVariantId = masterVariant.PwMasterVariantId,
                    ParentMasterVariant = masterVariant,
                    ShopifyVariantId = shopifyVariantId,
                    Sku = sku,
                    Title = VariantTitleCorrection(title),
                    IsPrimary = true,
                    IsActive = true, // Because it's in the live catalog!
                };

                variantRepository.InsertVariant(newVariant);
                masterVariant.Variants.Add(newVariant);

                return masterVariant;
            }
        }

        private static string VariantTitleCorrection(string title)
        {
            return
                (title.IsNullOrEmpty() || title.ToUpper() == "DEFAULT TITLE" || title.ToUpper() == "DEFAULT")
                    ? VariantDefaultTitle
                    : title;
        }


        public PwVariant FindVariant(PwMasterVariant masterVariant, string title, string sku)
        {
            var titleSearch = VariantTitleCorrection(title);

            return masterVariant.Variants.FirstOrDefault(
                    x => x.Sku == sku && VariantTitleCorrection(x.Title) == titleSearch);
        }
    }
}
