using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model;
using Push.Foundation.Utilities.Helpers;
using Push.Utilities.Helpers;

namespace ProfitWise.Data.Services
{
    public static class MatchingExtensions
    {

        // This expresses one of the Golden Rules - the System will auto-consolidate Products only
        // when Title and Vendor match. AND, it is impossible for two Products with the same Title and Vendor
        // to exist under different Master Product
        public static PwMasterProduct FindMasterProduct(
                this IList<PwMasterProduct> masterProducts, string title, string vendor)
        {
            var firstOrDefault = masterProducts
                .SelectMany(x => x.Products)
                .FirstOrDefault(x => x.Title == title && x.Vendor == vendor);
            return firstOrDefault?.ParentMasterProduct;
        }

        public static PwProduct FindProduct(
                this PwMasterProduct masterProduct, string title, string vendor, long? shopifyProductId)
        {
            return
                masterProduct
                    .Products
                    .FirstOrDefault(x => x.Title == title &&
                                    x.Vendor == vendor &&
                                    x.ShopifyProductId == shopifyProductId);
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
            return
                masterVariants
                    .SelectMany(x => x.Variants)
                    .Where(x => x.ShopifyVariantId == shopifyVariantId)
                    .ToList();
        }

        public static PwMasterVariant FindMasterVariant(
                    this PwMasterProduct masterProduct, string sku, string title)
        {
            var firstOrDefault = masterProduct
                .MasterVariants
                .SelectMany(x => x.Variants)
                .FirstOrDefault(x => x.Sku == sku && 
                                    x.Title.VariantTitleCorrection() == title.VariantTitleCorrection());

            return firstOrDefault?.ParentMasterVariant;
        }

        public static PwVariant FindVariant(
                    this PwMasterVariant masterVariant, string sku, string title, long? shopifyVariantId)
        {
            return
                masterVariant.Variants.FirstOrDefault(
                    x => x.Sku == sku && 
                        x.Title.VariantTitleCorrection() == title.VariantTitleCorrection() && 
                        x.ShopifyVariantId == shopifyVariantId);
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
