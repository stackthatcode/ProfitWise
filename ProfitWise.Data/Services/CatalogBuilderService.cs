using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    [Intercept(typeof(ShopRequired))]
    public class CatalogBuilderService : IShopFilter
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly ConnectionWrapper _connectionWrapper;

        public PwShop PwShop { get; set; }

        // TODO => migrate to Preferences
        private const bool StockedDirectlyDefault = true;

        public CatalogBuilderService(
                IPushLogger logger, MultitenantFactory multitenantFactory, ConnectionWrapper connectionWrapper)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _connectionWrapper = connectionWrapper;
        }

        public IDbTransaction Transaction { get; set; }

        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.InitiateTransaction();            
        }

        public void CommitTransaction()
        {
            _connectionWrapper.CommitTranscation();
        }

        public PwMasterProduct CreateMasterProduct()
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

        public PwProduct CreateProduct(
                PwMasterProduct masterProduct, ProductBuildContext productBuildContext)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            _pushLogger.Debug(
                $"Create new Product: {productBuildContext.Title} (Id = {productBuildContext.ShopifyProductId})");

            // Create the new Product
            PwProduct finalProduct = new PwProduct();
            finalProduct.PwShopId = PwShop.PwShopId;
            finalProduct.PwMasterProductId = masterProduct.PwMasterProductId;
            finalProduct.ShopifyProductId = productBuildContext.ShopifyProductId;
            finalProduct.Title = productBuildContext.Title;
            finalProduct.Vendor = productBuildContext.Vendor ?? "";
            finalProduct.ProductType = productBuildContext.ProductType ?? "";
            finalProduct.Tags = productBuildContext.Tags;
            finalProduct.IsActive = productBuildContext.IsActive;
            finalProduct.IsPrimary = false;
            finalProduct.IsPrimaryManual = false;
            finalProduct.LastUpdated = DateTime.Now;
            finalProduct.ParentMasterProduct = masterProduct;
            
            var productId = productRepository.InsertProduct(finalProduct);
            finalProduct.PwProductId = productId;
            finalProduct.ParentMasterProduct = masterProduct;
            masterProduct.Products.Add(finalProduct);

            this.AutoUpdatePrimary(masterProduct);
            return finalProduct;
        }

        public PwMasterVariant CreateMasterVariant(VariantBuildContext context)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsService = this._multitenantFactory.MakeCogsService(this.PwShop);

            _pushLogger.Debug(
                $"Creating new Master Variant: {context.Title}, {context.Sku} " +
                $"(Id = {context.ShopifyVariantId})");

            // SPECIFICATION => intentionally set the new CoGS => $0, and the Currency Id => Shop Currency
            var masterVariant = new PwMasterVariant()
            {
                PwShopId = this.PwShop.PwShopId,
                PwMasterProductId = context.MasterProduct.PwMasterProductId,
                ParentMasterProduct = context.MasterProduct,
                Exclude = false,
                StockedDirectly = StockedDirectlyDefault,
                Variants = new List<PwVariant>(),

                CogsTypeId = CogsType.FixedAmount,
                CogsAmount = 0,
                CogsCurrencyId = this.PwShop.CurrencyId,
                CogsMarginPercent = null,
                CogsDetail = false,
            };

            masterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(masterVariant);
            cogsService.UpdateGoodsOnHandForMasterVariant(
                    CogsDateBlockContext.Make(masterVariant, this.PwShop.CurrencyId));

            _pushLogger.Debug($"Created new Master Variant: (Master Variant Id = {masterVariant.PwMasterVariantId})");
            return masterVariant;
        }

        public PwVariant CreateVariant(VariantBuildContext context)
        {
            _pushLogger.Debug($"Creating new Variant: {context.Title}, {context.Sku} (Id = {context.ShopifyVariantId})");
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);

            var newVariant = new PwVariant();
            newVariant.PwShopId = this.PwShop.PwShopId;
            newVariant.PwProductId = context.Product.PwProductId;  // This is a permanent association :-)
            newVariant.PwMasterVariantId = context.MasterVariant.PwMasterVariantId;
            newVariant.ParentMasterVariant = context.MasterVariant;
            newVariant.ShopifyProductId = context.ShopifyProductId;
            newVariant.ShopifyVariantId = context.ShopifyVariantId;
            newVariant.Sku = context.Sku;
            newVariant.Title = context.Title.VariantTitleCorrection();
            newVariant.LowPrice = 0m;      // defaults to "0", updated by 
            newVariant.HighPrice = 0m;
            newVariant.Inventory = null;
            newVariant.IsActive = context.IsActive;
            newVariant.IsPrimary = false;
            newVariant.IsPrimaryManual = false;
            newVariant.LastUpdated = DateTime.Now;

            newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
            context.MasterVariant.Variants.Add(newVariant);
            this.AutoUpdatePrimary(context.MasterVariant);
            return newVariant;
        }


        public void AutoUpdatePrimary(PwMasterProduct masterProduct)
        {
            var repository = _multitenantFactory.MakeProductRepository(this.PwShop);

            if (masterProduct.Products.Count == 0)
            {
                return;
            }

            var primaryProduct = masterProduct.AutoPrimaryProduct();
            primaryProduct.IsPrimary = true;

            masterProduct.Products
                .Where(x => x != primaryProduct)
                .ForEach(x => x.IsPrimary = false);

            foreach (var product in masterProduct.Products)
            {
                repository.UpdateProductIsPrimary(product);
            }
        }

        public void AutoUpdatePrimary(PwMasterVariant masterVariant)
        {
            var repository = _multitenantFactory.MakeVariantRepository(this.PwShop);

            if (masterVariant.Variants.Count == 0)
            {
                return;
            }
            var primaryVariant = masterVariant.AutoPrimaryVariant();

            primaryVariant.IsPrimary = true;
            masterVariant.Variants
                .Where(x => x != primaryVariant)
                .ForEach(x => x.IsPrimary = false);

            foreach (var variant in masterVariant.Variants)
            {
                repository.UpdateVariantIsPrimary(variant);
            }
        }


        // A Shopify Product or Variant Id may appear under multiple Master Products due to historical data.
        // This updates the Status so the most recent one is Active and all the historical ones Inactive.
        public void UpdateActiveProduct(IList<PwMasterProduct> allMasterProducts, long? shopifyProductId)
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

        public void UpdateActiveVariant(IList<PwMasterProduct> allMasterProducts, long? shopifyVariantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var variants = allMasterProducts
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

    }
}

