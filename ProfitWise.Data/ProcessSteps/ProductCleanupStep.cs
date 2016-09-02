using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.ProcessSteps
{
    public class ProductCleanupStep
    {
        private readonly IPushLogger _pushLogger;
        private readonly MultitenantFactory _multitenantFactory;
        private readonly RefreshServiceConfiguration _configuration;
        private readonly PwShopRepository _shopRepository;


        public ProductCleanupStep(
                IPushLogger logger,
                MultitenantFactory multitenantFactory,
                RefreshServiceConfiguration configuration,
                PwShopRepository shopRepository)
        {
            _pushLogger = logger;
            _multitenantFactory = multitenantFactory;
            _configuration = configuration;
            _shopRepository = shopRepository;
        }


        public virtual void Execute(ShopifyCredentials shopCredentials)
        {
            // Get Shopify Shop
            var shop = _shopRepository.RetrieveByUserId(shopCredentials.ShopOwnerId);

            // Create an instance of multi-tenant-aware repositories
            var service = this._multitenantFactory.MakeProductVariantService(shop);
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

            // Delete Orphaned Master Products/Variants
            _pushLogger.Debug($"Deleting Orphaned Master Products and Master Variants");
            productRepository.DeleteOrphanedMasterProducts();
            variantRepository.DeleteOrphanedMasterVariants();
        }

        private void RemoveInactiveWithoutReference(PwShop shop, 
                IList<PwMasterProduct> masterProductCatalog, IList<OrderLineItemSubset> orderLineItems)
        {
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);
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
                }
            }
        }

        private void UpdateInactiveVariantPrice(PwShop shop, 
                IList<PwMasterProduct> masterProductCatalog, IList<OrderLineItemSubset> orderLineItems)
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

                    _pushLogger.Debug($"Updating Variant {variant.PwVariantId} price range: {lowPrice} to {highPrice}");
                    variantRepository.UpdateVariantPrice(variant.PwVariantId, lowPrice, highPrice);
                }
            }
        }
    }
}

