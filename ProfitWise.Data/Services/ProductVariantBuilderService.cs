using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.Helpers;


namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class ProductVariantBuilderService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;

        public PwShop PwShop { get; set; }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;
        private const string VariantDefaultTitle = "Default Title";

        public ProductVariantBuilderService(
                    IPushLogger logger, MultitenantFactory multitenantFactory)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
        }
        

        public PwMasterProduct FindOrCreateMasterProduct(
                    IList<PwMasterProduct> masterProducts, string title, long shopifyProductId)
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


        public PwProduct FindOrCreateProduct(
                    PwMasterProduct masterProduct, 
                    string title, long? shopifyProductId, string vendor, string tags, string productType)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

            PwProduct productMatch =
                masterProduct
                    .Products
                    .FirstOrDefault(x => x.Title == title && 
                                    x.Vendor == vendor &&
                                    x.ShopifyProductId == shopifyProductId);

            if (productMatch != null)
            {
                _pushLogger.Debug(
                    $"Found existing Product: {title} by {vendor} (Id = {shopifyProductId}, " +
                    $"PwProductId = {productMatch.PwProductId})");

                productMatch.Tags = tags;
                productMatch.ProductType = productType;
                productMatch.LastUpdated = DateTime.Now;

                productRepository.UpdateProduct(productMatch); // TODO UPDATE ME!!!
                return productMatch;
            }
            else
            {
                _pushLogger.Debug(
                    $"Create new Product: {title} (Id = {shopifyProductId})");

                // Create the new Product
                PwProduct finalProduct = new PwProduct()
                {
                    PwShopId = PwShop.PwShopId,
                    PwMasterProductId = masterProduct.PwMasterProductId,
                    ShopifyProductId = shopifyProductId,
                    Title = title,
                    Vendor = vendor,
                    ProductType = productType,
                    IsActive = null,
                    IsPrimary = null,
                    IsManuallySelected = false,
                    LastUpdated = DateTime.Now,
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

        public void SetActiveProductByShopifyProductId(IList<PwMasterProduct> allMasterProducts, int shopifyProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var products =
                allMasterProducts
                    .SelectMany(x => x.Products)
                    .Where(x => x.ShopifyProductId == shopifyProductId)
                    .ToList();

            var activeProduct = products.OrderByDescending(x => x.LastUpdated).First();
            activeProduct.IsActive = true;
            products.Where(x => x != activeProduct).ForEach(x => x.IsActive = false);

            foreach (var product in products)
            {
                productRepository.UpdateProduct(product);
            }

        }


        public PwMasterVariant FindOrCreateMasterVariant(
                PwProduct product, bool isActive, string title, long? shopifyProductId, long? shopifyVariantId, 
                string sku)
        {
            var titleSearch = VariantTitleCorrection(title);
            var masterProduct = product.ParentMasterProduct;

            var matchingVariantBySkuAndTitle =
                masterProduct.MasterVariants
                    .SelectMany(x => x.Variants)
                    .FirstOrDefault(x => x.Sku == sku && 
                            VariantTitleCorrection(x.Title) == titleSearch);

            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);


            if (matchingVariantBySkuAndTitle != null)
            {
                _pushLogger.Debug(
                    $"Found existing Master Variant: {title}, {sku} " +
                    $"(Id = {shopifyVariantId}), PwMasterVariantId = {matchingVariantBySkuAndTitle.PwMasterVariantId}, " + 
                    $"PwVariantId = {matchingVariantBySkuAndTitle.PwVariantId})");

                return matchingVariantBySkuAndTitle.ParentMasterVariant;
            }
            else
            {
                _pushLogger.Debug(
                    $"Creating new Master Variant & Variant: {title}, {sku} " +
                    $"(Id = {shopifyVariantId})");
                
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
                    ShopifyProductId = shopifyProductId,
                    ShopifyVariantId = shopifyVariantId,
                    Sku = sku,
                    Title = VariantTitleCorrection(title),
                    LowPrice = 0m,      // defaults to "0", updated by 
                    HighPrice = 0m,
                    Inventory = null,
                    IsPrimary = true,
                    IsActive = isActive, 
                };

                newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
                masterVariant.Variants.Add(newVariant);

                return masterVariant;
            }
        }


        public IList<PwMasterProduct> RetrieveFullCatalog()
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            masterProductCatalog.LoadMasterVariants(masterVariants);

            return masterProductCatalog;
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
