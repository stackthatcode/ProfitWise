using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Catalog
{
    public static class MatchingExtensions
    {
        // This expresses one of the Golden Rules - the System will auto-consolidate Products only
        // when Title and Vendor match. AND, it is impossible for two Products with the same Title and Vendor
        // to exist under different Master Product
        public static PwMasterProduct FindMasterProduct(
                this IList<PwMasterProduct> masterProducts, ProductBuildContext context)
        {
            var firstOrDefault = masterProducts
                .SelectMany(x => x.Products)
                .FirstOrDefault(x => x.Title == context.Title && x.Vendor == context.Vendor);
            return firstOrDefault?.ParentMasterProduct;
        }

        public static PwProduct FindProduct(
                this PwMasterProduct masterProduct, ProductBuildContext context)
        {
            return masterProduct
                    .Products.FirstOrDefault(
                                x => x.Title == context.Title &&
                                    x.Vendor == context.Vendor &&
                                    x.ShopifyProductId == context.ShopifyProductId);
        }

        public static PwMasterVariant FindMasterVariant(this PwMasterProduct masterProduct, VariantBuildContext context)
        {
            return masterProduct.FindMasterVariant(context.Sku, context.Title);
        }

        public static PwMasterVariant FindMasterVariant(this PwMasterProduct masterProduct, string sku, string title)
        {
            var firstOrDefault = masterProduct
                .MasterVariants
                .SelectMany(x => x.Variants)
                .FirstOrDefault(x => 
                        String.Equals(x.Title.VariantTitleCorrection(), title.VariantTitleCorrection(), 
                        StringComparison.OrdinalIgnoreCase));
            // x.Sku == sku && 
            return firstOrDefault?.ParentMasterVariant;
        }

        public static PwVariant FindVariant(this PwMasterVariant masterVariant, VariantBuildContext context)
        {
            return masterVariant.Variants.FirstOrDefault(
                        x => x.Sku == context.Sku &&
                        String.Equals(x.Title.VariantTitleCorrection(), context.Title.VariantTitleCorrection(),
                        StringComparison.OrdinalIgnoreCase) && 
                        x.ShopifyVariantId == context.ShopifyVariantId);
        }


        public static IList<PwProduct> FindProductByShopifyId(
                this IList<PwMasterProduct> masterProducts, long? shopifyProductId)
        {
            return
                masterProducts
                    .SelectMany(x => x.Products)
                    .Where(x => x.ShopifyProductId == shopifyProductId)
                    .ToList();
        }

        public static IList<PwVariant> FindVariantsByShopifyId(
                this IEnumerable<PwMasterVariant> masterVariants, long? shopifyVariantId)
        {
            return masterVariants
                    .SelectMany(x => x.Variants)
                    .Where(x => x.ShopifyVariantId == shopifyVariantId)
                    .ToList();
        }


        private const string VariantDefaultTitle = "Default Title";

        public static string VariantTitleCorrection(this string title)
        {
            return
                (title.IsNullOrEmpty() || title.ToUpper() == "DEFAULT TITLE" || title.ToUpper() == "DEFAULT")
                    ? VariantDefaultTitle
                    : title;
        }
    }
}
