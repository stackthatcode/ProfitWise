using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.General;
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
                .FirstOrDefault(x => x.Title.CaselessEquals(context.Title) &&
                                    x.Vendor.CaselessEquals(context.Vendor));

            return firstOrDefault?.ParentMasterProduct;
        }

        public static PwProduct FindProduct(
                this PwMasterProduct masterProduct, ProductBuildContext context)
        {
            return masterProduct
                    .Products.FirstOrDefault(
                                x => x.Title.CaselessEquals(context.Title) &&
                                    x.Vendor.CaselessEquals(context.Vendor) &&
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
                    x.Title.VariantTitleCorrection().CaselessEquals(title.VariantTitleCorrection()));
                       // && x.Sku.CaselessEquals(sku));
            return firstOrDefault?.ParentMasterVariant;
        }

        public static PwVariant FindVariant(this PwMasterVariant masterVariant, VariantBuildContext context)
        {
            return masterVariant.Variants.FirstOrDefault(
                        x =>  //x.Sku.CaselessEquals(context.Sku) &&
                         x.Title.VariantTitleCorrection().CaselessEquals(context.Title.VariantTitleCorrection())
                         && x.ShopifyVariantId == context.ShopifyVariantId);
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
