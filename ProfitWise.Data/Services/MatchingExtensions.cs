using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model;
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
                this PwMasterProduct masterProduct, string title, string vendor, long shopifyProductId)
        {
            return
                masterProduct
                    .Products
                    .FirstOrDefault(x => x.Title == title &&
                                    x.Vendor == vendor &&
                                    x.ShopifyProductId == shopifyProductId);
        }

        public static PwMasterVariant FindMasterVariant(
                    this PwMasterProduct masterProduct, string sku, string title)
        {
            var firstOrDefault = masterProduct
                .MasterVariants
                .SelectMany(x => x.Variants)
                .FirstOrDefault(x =>    x.Sku == sku && 
                                        VariantTitleCorrection(x.Title) == title);

            return firstOrDefault?.ParentMasterVariant;
        }

        public static PwVariant FindVariant(
                    this PwMasterVariant masterVariant, string sku, string title, long shopifyVariantId)
        {
            return
                masterVariant.Variants.FirstOrDefault(
                    x => x.Sku == sku && 
                        x.Title.VariantTitleCorrection() == title.VariantTitleCorrection() && 
                        x.ShopifyVariantId == shopifyVariantId);
        }



        //public static bool Same(
        //            this PwVariant variant1, PwVariant variant2)
        //{
        //}

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
