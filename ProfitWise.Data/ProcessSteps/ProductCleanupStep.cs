using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Shop;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductCleanupStep
    {
        private readonly BatchLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly ShopRepository _shopRepository;


        public ProductCleanupStep(
                BatchLogger logger,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                ShopRepository shopRepository)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _configuration = configuration;
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

            // TODO - this is on hold for now
            // Remove Inactive Products and Variants that don't reference Order Line Items
            // RemoveInactiveWithoutReference(shop, masterProductCatalog, orderLineItems);

            // Update Inactive Variant prices using Order Line Items
            UpdateInactiveVariantPrice(shop, masterProductCatalog, orderLineItems);

            // Deletes any orphaned Master Products or Master Variants
            _pushLogger.Debug($"Deleting Orphaned Master Products and Master Variants");
            productRepository.DeleteOrphanedMasterProducts();
            variantRepository.DeleteChildlessMasterVariants();
        }

        private void RemoveInactiveWithoutReference(
                PwShop shop, IList<PwMasterProduct> masterProductCatalog, IList<OrderLineItemSubset> orderLineItems)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);
            var catalogService = this._multitenantFactory.MakeCatalogBuilderService(shop);

            var masterVariants = masterProductCatalog.SelectMany(x => x.MasterVariants).ToList();

            foreach (var product in 
                        masterProductCatalog
                            .SelectMany(x => x.Products)
                            .Where(x => x.IsActive == false)
                            .ToList())
            {
                if (orderLineItems.All(x => x.PwProductId != product.PwProductId))
                {
                    _pushLogger.Debug($"Inactive Product {product.Title}, {product.PwProductId} " +
                                      "is being removed - no referring Order Line Items");
                    product.ParentMasterProduct.Products.Remove(product);

                    productRepository.DeleteProduct(product.PwProductId);

                    catalogService.AutoUpdatePrimary(product.ParentMasterProduct);
                }
            }

            foreach (var variant in 
                        masterVariants
                            .SelectMany(x => x.Variants)
                            .Where(x => x.IsActive == false)
                            .ToList())
            {
                if (orderLineItems.All(x => x.PwProductId != variant.PwProductId))
                {
                    _pushLogger.Debug($"Inactive Variant {variant.Title}, {variant.Sku}, {variant.PwVariantId} " +
                                      "is being removed - no referring Order Line Items");
                    variant.ParentMasterVariant.Variants.Remove(variant);

                    variantRepository.DeleteVariantByVariantId(variant.PwVariantId);
                    catalogService.AutoUpdatePrimary(variant.ParentMasterVariant);
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
                    variantRepository.UpdateVariantPriceAndInventory(variant.PwVariantId, lowPrice, highPrice, null);
                }
            }
        }
    }
}

