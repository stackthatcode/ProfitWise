using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Catalog
{
    public static class FindingUtilityExtensions
    {        
        public static IList<PwProduct> FindProductByShopifyId(
                this IList<PwMasterProduct> masterProducts, long? shopifyProductId)
        {
            return masterProducts
                    .SelectMany(x => x.Products)
                    .Where(x => x.ShopifyProductId == shopifyProductId)
                    .ToList();
        }

        public static IList<PwVariant> FindVariantsByShopifyVariantId(
                this IEnumerable<PwMasterVariant> masterVariants, long? shopifyVariantId)
        {
            return masterVariants
                    .SelectMany(x => x.Variants)
                    .Where(x => x.ShopifyVariantId == shopifyVariantId)
                    .ToList();
        }

        public static IList<PwProduct> FindProductsByShopifyId(this PwMasterProduct masterProduct, long? shopifyProductId)
        {
            return masterProduct.Products.Where(x => x.ShopifyProductId == shopifyProductId).ToList();
        }

        public static IList<PwMasterProduct> FindMasterProductsByShopifyId(
                this IList<PwMasterProduct> masterProducts, long? shopifyProductId)
        {
            return masterProducts.Where(
                    x => x.Products.Any(product => product.ShopifyProductId == shopifyProductId))
                .ToList();
        }

        public static IList<PwVariant> FindVariantsByShopifyProductId(
                this IEnumerable<PwMasterProduct> masterProducts, long? shopifyProductId)
        {
            return masterProducts
                .SelectMany(x => x.MasterVariants)
                .SelectMany(x => x.Variants)
                .Where(x => x.ShopifyProductId == shopifyProductId)
                .ToList();
        }


        public static IList<PwVariant> FindInactiveVariants(this IList<PwMasterProduct> masterProductCatalog)
        {
            var masterVariants = masterProductCatalog.SelectMany(x => x.MasterVariants).ToList();
            return masterVariants
                .SelectMany(x => x.Variants)
                .Where(x => x.IsActive == false)
                .ToList();
        }

        public static IList<PwProduct> FindInactiveProducts(this IList<PwMasterProduct> masterProducts)
        {
            return masterProducts.SelectMany(x => x.Products).Where(x => x.IsActive == false).ToList();
        }
    }
}
