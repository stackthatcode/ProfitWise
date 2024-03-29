﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Castle.Core.Internal;
using ProfitWise.Data.Aspect;
using ProfitWise.Data.Database;
using ProfitWise.Data.Factories;
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
        
        public PwProduct CreateProductAndAssignToMaster(
                    ProductBuildContext productBuildContext, PwMasterProduct masterProduct)
        {
            _pushLogger.Debug($"Create new Product: {productBuildContext.Title} (Id = {productBuildContext.ShopifyProductId})");

            // Create and save new Product
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
            finalProduct.LastUpdated = DateTime.UtcNow;

            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var productId = productRepository.InsertProduct(finalProduct);
            
            finalProduct.PwProductId = productId;
            finalProduct.ParentMasterProduct = masterProduct;
            masterProduct.Products.Add(finalProduct);

            this.AutoUpdateAndSavePrimary(masterProduct);
            return finalProduct;
        }

        public void UpdateProduct(ProductBuildContext context, PwProduct product)
        {
            _pushLogger.Debug($"Updating existing Product: {context.Title}");

            product.Tags = context.Tags;
            product.ProductType = context.ProductType;
            product.LastUpdated = DateTime.UtcNow;

            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            productRepository.UpdateProduct(product);
        }

        // A Shopify Product Id may appear under multiple Master Products due to historical data.
        // This updates the Status so the most recent one is Active and all the historical ones Inactive.
        public void UpdateActiveProductAcrossCatalog(ProductBuildContext productBuildContext)
        {
            if (!productBuildContext.ShopifyProductId.HasValue)
            {
                return;
            }

            // Locate all Products with ShopifyProductId across all Master Products
            var products = 
                productBuildContext
                    .ExistingMasterProducts
                    .FindProductByShopifyId(productBuildContext.ShopifyProductId);
            if (products.Count == 1)
            {
                return;
            }

            // The Active Product is the most recently updated
            var activeProduct = products.OrderByDescending(x => x.LastUpdated).First();
            activeProduct.IsActive = true;
            products.Where(x => x != activeProduct).ForEach(x => x.IsActive = false);

            _pushLogger.Debug($"Updated Active Shopify Product to {activeProduct.PwProductId}");
            
            // Write all of the IsActive statuses to persistence
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            foreach (var product in products)
            {
                productRepository.UpdateProductIsActive(product);
            }
        }
        
        public PwMasterVariant CreateAndAssignMasterVariant(VariantBuildContext context)
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
                CogsAmount = context.Cost,
                CogsCurrencyId = this.PwShop.CurrencyId,
                CogsMarginPercent = null,
                CogsDetail = false,
            };

            masterVariant.PwMasterVariantId = variantRepository.InsertMasterVariant(masterVariant);

            context.MasterProduct.MasterVariants.Add(masterVariant);

            // Update CoGS data that will be used downstream
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
            newVariant.PwMasterVariantId = context.TargetMasterVariant.PwMasterVariantId;
            newVariant.ParentMasterVariant = context.TargetMasterVariant;
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
            newVariant.LastUpdated = DateTime.UtcNow;

            newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
            context.TargetMasterVariant.Variants.Add(newVariant);

            // Critical - updates the IsPrimary Variant
            this.AutoUpdateAndSavePrimary(context.TargetMasterVariant);
            return newVariant;
        }
        
        public void AutoUpdateAndSavePrimary(PwMasterProduct masterProduct)
        {
            masterProduct.AutoUpdatePrimary();

            var repository = _multitenantFactory.MakeProductRepository(this.PwShop);
            foreach (var product in masterProduct.Products)
            {
                repository.UpdateProductIsPrimary(product);
            }
        }

        public void AutoUpdateAndSavePrimary(PwMasterVariant masterVariant)
        {
            var repository = _multitenantFactory.MakeVariantRepository(this.PwShop);

            if (masterVariant.Variants.Count == 0)
            {
                return;
            }
            var primaryVariant = masterVariant.AutoSelectPrimary();

            primaryVariant.IsPrimary = true;
            masterVariant.Variants.Where(x => x != primaryVariant).ForEach(x => x.IsPrimary = false);

            foreach (var variant in masterVariant.Variants)
            {
                repository.UpdateVariantIsPrimary(variant);
            }
        }
        
        public void UpdateActiveVariantAcrossCatalog(IList<PwMasterProduct> allMasterProducts, long? shopifyVariantId)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);

            // IsActive simply flags the most recent instance of a Variant
            // This query will illustrate the behavior of IsActive:
            // SELECT PwShopId, ShopifyVariantId, COUNT(*)
            // FROM profitwisevariant WHERE IsActive = 1
            // GROUP BY PwShopId, ShopifyVariantId ORDER BY COUNT(*) DESC;

            var variants = allMasterProducts
                .SelectMany(x => x.MasterVariants)
                .FindVariantsByShopifyVariantId(shopifyVariantId);

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
        
        public void FlagMissingVariantsAsInactive(
                IList<PwMasterProduct> allMasterProducts, 
                long? shopifyProductId, 
                List<long> activeShopifyVariantIds)
        {
            // Mark all Variants as InActive that aren't in the import
            var variantRepository = _multitenantFactory.MakeVariantRepository(this.PwShop);
            var catalogService = _multitenantFactory.MakeCatalogBuilderService(PwShop);

            // Locate all Variants with the Product's ShopifyProductId across all Master Products and Products
            // NOTE: there are Variants with ShopifyProductId but without ShopifyVariantId
            var allVariantsForShopifyProduct =
                allMasterProducts
                    .FindVariantsByShopifyProductId(shopifyProductId)
                    .Where(x => x.ShopifyVariantId != null)
                    .ToList();

            // Identify Variants that are not in the list of Active Shopify Variants from the latest import
            var missingFromActive =
                allVariantsForShopifyProduct                    
                    .Where(x => activeShopifyVariantIds.All(activeId => activeId != x.ShopifyVariantId))
                    .ToList();

            missingFromActive.ForEach(variant =>
            {
                _pushLogger.Debug($"Flagging PwVariantId {variant.PwVariantId} as Inactive");

                variant.IsActive = false;
                variantRepository.UpdateVariantIsActive(variant);
                catalogService.AutoUpdateAndSavePrimary(variant.ParentMasterVariant);
            });
        }
    }
}

