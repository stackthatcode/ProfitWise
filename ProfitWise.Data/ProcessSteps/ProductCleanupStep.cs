using System.Linq;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Factories;
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
            var productRepository = this._multitenantFactory.MakeProductRepository(shop);
            var variantRepository = this._multitenantFactory.MakeVariantRepository(shop);
            var orderRepository = this._multitenantFactory.MakeShopifyOrderRepository(shop);

            // Get all existing Variant and ProfitWise Products
            var masterVariants = variantRepository.RetrieveAllMasterVariants();
            var masterProductCatalog = productRepository.RetrieveAllMasterProducts();
            masterProductCatalog.LoadMasterVariants(masterVariants);

            var mappings = orderRepository.RetrieveLineItemProfitWiseMapping();

            foreach (var product in 
                        masterProductCatalog
                            .SelectMany(x => x.Products)
                            .Where(x => x.IsActive == false)
                            .ToList())
            {
                if (mappings.All(x => x.PwProductId != product.PwProductId))
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
                if (mappings.All(x => x.PwProductId != variant.PwProductId))
                {
                    _pushLogger.Debug($"Inactive Variant {variant.Title}, {variant.Sku}, {variant.PwVariantId} " + 
                                    "is being removed - no referring Order Line Items");
                    variant.ParentMasterVariant.Variants.Remove(variant);

                    variantRepository.DeleteVariantByVariantId(variant.PwVariantId);
                }
            }

            _pushLogger.Debug($"Deleting Orphaned Master Products and Master Variants");
            productRepository.DeleteOrphanedMasterProducts();
            variantRepository.DeleteOrphanedMasterVariants();
        }
    }
}

