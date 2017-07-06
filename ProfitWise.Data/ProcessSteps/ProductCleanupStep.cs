using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories.System;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductCleanupStep
    {
        private readonly BatchLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly ShopRepository _shopRepository;


        public ProductCleanupStep(
                BatchLogger logger, MultitenantFactory multitenantFactory, ShopRepository shopRepository)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerUserId);

            // Create an instance of multi-tenant-aware repositories
            var service = this._multitenantFactory.MakeCatalogRetrievalService(shop);
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);
            var orderRepository = this._multitenantFactory.MakeShopifyOrderRepository(shop);

            // Get all existing ProfitWise Products and Order Line Items
            var masterProductCatalog = service.RetrieveFullCatalog();
            var orderLineItems = orderRepository.RetrieveLineItemSubset();

            // Remove Inactive Products and Variants that don't reference Order Line Items
            RemoveInactiveWithoutReference(shop, masterProductCatalog, orderLineItems);

            // Update Inactive Variant prices using Order Line Items
            UpdateInactiveVariantPrice(shop, masterProductCatalog, orderLineItems);

            // Deletes any childless Master Products or Master Variants
            _pushLogger.Debug($"Deleting Childless Master Products and Master Variants");
            productRepository.DeleteChildlessMasterProducts();
            variantRepository.DeleteChildlessMasterVariants();
        }

        private void RemoveInactiveWithoutReference(
                    PwShop shop, IList<PwMasterProduct> masterProductCatalog, IList<OrderLineItemSubset> orderLineItems)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(shop);

            foreach (var variant in masterProductCatalog.FindInactiveVariants())
            {
                if (orderLineItems.All(x => x.PwVariantId != variant.PwVariantId))
                {
                    _pushLogger.Debug($"Inactive Variant {variant.Title}, {variant.Sku}, {variant.PwVariantId} " +
                                      "is being removed - no referring Order Line Items");
                    variant.ParentMasterVariant.Variants.Remove(variant);

                    using (var transaction = variantRepository.InitiateTransaction())
                    {
                        variantRepository.DeleteVariantByVariantId(variant.PwVariantId);
                        catalogService.AutoUpdateAndSavePrimary(variant.ParentMasterVariant);
                        transaction.Commit();
                    }
                }
            }

            // The presumption here is that the Inactive Variants *should* have been removed already
            var inactiveProducts = masterProductCatalog.FindInactiveProducts();
            foreach (var product in inactiveProducts)
            {
                if (orderLineItems.All(x => x.PwProductId != product.PwProductId))
                {
                    _pushLogger.Debug(
                        $"Inactive Product {product.Title}, {product.PwProductId} " +
                        "is being removed - no referring Order Line Items");

                    product.ParentMasterProduct.Products.Remove(product);

                    using (var transaction = productRepository.InitiateTransaction())
                    {
                        productRepository.DeleteProduct(product.PwProductId);
                        catalogService.AutoUpdateAndSavePrimary(product.ParentMasterProduct);
                        transaction.Commit();
                    }
                }
            }
        }

        private void UpdateInactiveVariantPrice(
                PwShop shop, IList<PwMasterProduct> masterProductCatalog, 
                IList<OrderLineItemSubset> orderLineItems)
        {
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);

            var variants =
                masterProductCatalog
                    .SelectMany(x => x.MasterVariants)
                    .SelectMany(x => x.Variants)
                    .Where(x => x.IsActive == false)
                    .ToList();

            foreach (var variant in variants)
            {
                var relatedLineItems = 
                    orderLineItems
                        .Where(x => x.PwVariantId == variant.PwVariantId).ToList();

                if (relatedLineItems.Any())
                {
                    var lowPrice = relatedLineItems.Min(x => x.UnitPrice);
                    var highPrice = relatedLineItems.Max(x => x.UnitPrice);

                    _pushLogger.Debug($"Updating (inactive) Variant {variant.PwVariantId} price range: {lowPrice} to {highPrice} and setting inventory to NULL");
                    variantRepository.UpdateVariant(
                        variant.PwVariantId, lowPrice, highPrice, variant.Sku, null, DateTime.UtcNow);
                }
            }
        }
    }
}

