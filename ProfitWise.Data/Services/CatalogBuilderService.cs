using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CatalogBuilderService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;

        public PwShop PwShop { get; set; }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;

        public CatalogBuilderService(
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
                    bool isActive, string title, long? shopifyProductId, string vendor, string tags, string productType)
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
                Tags = tags,
                IsActive = isActive,
                IsPrimary = false,
                IsPrimaryManual = false,
                LastUpdated = DateTime.Now,
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
                PwMasterVariant masterVariant, bool isActive, PwProduct product, string title, long? shopifyVariantId, string sku)
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
                IsActive = isActive,
                IsPrimary = false,
                IsPrimaryManual = false,
                LastUpdated = DateTime.Now,
            };

            newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
            masterVariant.Variants.Add(newVariant);
            return newVariant;
        }


        public void UpdatePrimaryProduct(PwMasterProduct masterProduct)
        {
            var repository = _multitenantFactory.MakeProductRepository(this.PwShop);

            if (masterProduct.Products.Count == 0)
            {
                return;
            }
            var primaryProduct = masterProduct.DeterminePrimaryProduct();
            primaryProduct.IsPrimary = true;
            masterProduct.Products
                .Where(x => x != primaryProduct)
                .ForEach(x => x.IsPrimary = false);

            foreach (var product in masterProduct.Products)
            {
                repository.UpdateProductIsPrimary(product);
            }
        }

        public void UpdateActiveShopifyProduct(IList<PwMasterProduct> allMasterProducts, long? shopifyProductId)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var products = allMasterProducts.FindProductByShopifyId(shopifyProductId);
            if (products.Count == 1)
            {
                return;
            }

            var activeProduct = products.OrderByDescending(x => x.LastUpdated).First();
            _pushLogger.Debug($"Updating Active Shopify Product to {activeProduct.PwProductId}");

            activeProduct.IsActive = true;
            products.Where(x => x != activeProduct).ForEach(x => x.IsActive = false);

            foreach (var product in products)
            {
                productRepository.UpdateProductIsActive(product);
            }
        }

        public void UpdateActiveShopifyVariant(IList<PwMasterProduct> allMasterProducts, long? shopifyVariantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var variants =
                allMasterProducts
                    .SelectMany(x => x.MasterVariants)
                    .FindVariantsByShopifyId(shopifyVariantId);
            if (variants.Count == 1)
            {
                return;
            }

            var activeVariant = variants.OrderByDescending(x => x.LastUpdated).First();

            _pushLogger.Debug($"Updating Active Shopify Variant to {activeVariant.PwVariantId}");

            activeVariant.IsActive = true;
            variants.Where(x => x != activeVariant).ForEach(x => x.IsActive = false);

            foreach (var variant in variants)
            {
                variantRepository.UpdateVariantIsActive(variant);
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
