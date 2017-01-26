using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
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
using Push.Utilities.General;


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
                IPushLogger logger, 
                MultitenantFactory multitenantFactory,
                ConnectionWrapper connectionWrapper)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _connectionWrapper = connectionWrapper;
        }


        public IDbTransaction InitiateTransaction()
        {
            return _connectionWrapper.StartTransactionForScope();            
        }

        public IDbTransaction Transaction { get; set; }



        public IList<PwMasterProduct> RetrieveFullCatalog()
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            var variantDataRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            var cogsRepository = this._multitenantFactory.MakeCogsEntryRepository(this.PwShop);            

            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            var masterVariants = variantDataRepository.RetrieveAllMasterVariants();
            var cogsDetails = cogsRepository.RetrieveCogsDetailAll();

            masterProductCatalog.LoadMasterVariants(masterVariants);
            masterVariants.LoadCogsDetail(cogsDetails);

            return masterProductCatalog;
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

        public PwProduct BuildAndSaveProduct(
                PwMasterProduct masterProduct, ProductBuildContext productBuildContext)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(this.PwShop);
            _pushLogger.Debug(
                $"Create new Product: {productBuildContext.Title} (Id = {productBuildContext.ShopifyProductId})");

            // Create the new Product
            PwProduct finalProduct = new PwProduct()
            {
                PwShopId = PwShop.PwShopId,
                PwMasterProductId = masterProduct.PwMasterProductId,
                ShopifyProductId = productBuildContext.ShopifyProductId,
                Title = productBuildContext.Title,
                Vendor = productBuildContext.Vendor,
                ProductType = productBuildContext.ProductType,
                Tags = productBuildContext.Tags,
                IsActive = productBuildContext.IsActive,
                IsPrimary = false,
                IsPrimaryManual = false,
                LastUpdated = DateTime.Now,
                ParentMasterProduct = masterProduct,
            };

            var productId = productRepository.InsertProduct(finalProduct);
            finalProduct.PwProductId = productId;
            finalProduct.ParentMasterProduct = masterProduct;
            masterProduct.Products.Add(finalProduct);

            this.UpdatePrimaryProduct(masterProduct);
            return finalProduct;
        }

        public PwMasterVariant BuildAndSaveMasterVariant(VariantBuildContext context)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
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
            _pushLogger.Debug($"Created new Master Variant: (Master Variant Id = {masterVariant.PwMasterVariantId})");
            return masterVariant;
        }

        public PwVariant BuildAndSaveVariant(VariantBuildContext context)
        {
            _pushLogger.Debug($"Creating new Variant: {context.Title}, {context.Sku} (Id = {context.ShopifyVariantId})");
            var variantRepository = this._multitenantFactory.MakeVariantRepository(this.PwShop);
            
            var newVariant = new PwVariant()
            {
                PwShopId = this.PwShop.PwShopId,
                PwProductId = context.Product.PwProductId,  // This is a permanent association :-)
                PwMasterVariantId = context.MasterVariant.PwMasterVariantId,
                ParentMasterVariant = context.MasterVariant,
                ShopifyProductId = context.ShopifyProductId,
                ShopifyVariantId = context.ShopifyVariantId,
                Sku = context.Sku,
                Title = context.Title.VariantTitleCorrection(),
                LowPrice = 0m,      // defaults to "0", updated by 
                HighPrice = 0m,
                Inventory = null,
                IsActive = context.IsActive,
                IsPrimary = false,
                IsPrimaryManual = false,
                LastUpdated = DateTime.Now,
            };

            newVariant.PwVariantId = variantRepository.InsertVariant(newVariant);
            context.MasterVariant.Variants.Add(newVariant);
            this.UpdatePrimaryVariant(context.MasterVariant);
            return newVariant;
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

        public void UpdatePrimaryVariant(PwMasterVariant masterVariant)
        {
            var repository = _multitenantFactory.MakeVariantRepository(this.PwShop);

            if (masterVariant.Variants.Count == 0)
            {
                return;
            }
            var primaryVariant = masterVariant.DeterminePrimaryVariant();

            primaryVariant.IsPrimary = true;
            masterVariant.Variants
                .Where(x => x != primaryVariant)
                .ForEach(x => x.IsPrimary = false);

            foreach (var variant in masterVariant.Variants)
            {
                repository.UpdateVariantIsPrimary(variant);
            }
        }

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

        public void UpdateActiveShopifyVariant(IList<PwMasterProduct> allMasterProducts, long? shopifyVariantId)
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

