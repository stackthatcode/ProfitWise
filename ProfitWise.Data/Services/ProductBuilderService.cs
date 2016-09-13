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
    public class ProductBuilderService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;

        public PwShop PwShop { get; set; }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;

        public ProductBuilderService(
                    IPushLogger logger, MultitenantFactory multitenantFactory)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
        }
        

        public PwMasterProduct BuildAndSaveMasterProduct()
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var masterProduct = new PwMasterProduct()
            {
                PwShopId = this.PwShop.PwShopId,
                Products = new List<PwProduct>(),
            };

            var masterProductId = productRepository.InsertMasterProduct(masterProduct);
            masterProduct.PwMasterProductId = masterProductId;
            _pushLogger.Debug($"Created new Master Product: (Master Product Id = {masterProductId})");
            return masterProduct;
        }

        public PwProduct BuildAndSaveProduct(PwMasterProduct masterProduct,
                    string title, long? shopifyProductId, string vendor, string tags, string productType)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);

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

        public void UpdateExistingProduct(PwProduct product, string tags, string productType)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            _pushLogger.Debug($"Updating existing Product: {product.Title}");

            product.Tags = tags;
            product.ProductType = productType;
            product.LastUpdated = DateTime.Now;

            productRepository.UpdateProduct(product); // TODO UPDATE ME!!!
        }

        public PwMasterVariant BuildAndSaveMasterVariant(
            PwProduct product, string title, long? shopifyProductId, long? shopifyVariantId,
            string sku)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var masterProduct = product.ParentMasterProduct;

            _pushLogger.Debug(
                $"Creating new Master Variant: {title}, {sku} (Id = {shopifyVariantId})");

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
            _pushLogger.Debug($"Created new Master Variant: (Master Variant Id = {masterVariant.PwMasterVariantId})");
            return masterVariant;
        }

        public PwVariant BuildAndSaveVariant(
                PwMasterVariant masterVariant,
                PwProduct product, string title, long? shopifyVariantId,
                string sku)
        {
            _pushLogger.Debug(
                $"Creating new Variant: {title}, {sku} (Id = {shopifyVariantId})");

            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            
            var newVariant = new PwVariant()
            {
                PwShopId = this.PwShop.PwShopId,
                PwProductId = product.PwProductId,  // This is a permanent association :-)
                PwMasterVariantId = masterVariant.PwMasterVariantId,
                ParentMasterVariant = masterVariant,
                ShopifyProductId = product.ShopifyProductId,
                ShopifyVariantId = shopifyVariantId,
                Sku = sku,
                Title = title.VariantTitleCorrection(),
                LowPrice = 0m,      // defaults to "0", updated by 
                HighPrice = 0m,
                Inventory = null,
                IsPrimary = true,
                IsActive = true, 
            };

            newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
            masterVariant.Variants.Add(newVariant);
            return newVariant;
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

        public IList<PwMasterProduct> RetrieveFullCatalog()
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            masterProductCatalog.LoadMasterVariants(masterVariants);

            return masterProductCatalog;
        }
    }
}
