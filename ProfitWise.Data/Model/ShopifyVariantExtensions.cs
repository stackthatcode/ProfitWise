namespace ProfitWise.Data.Model
{
    public static class ShopifyVariantExtensions
    {
        public static bool ExactMatchToLineItem(this ShopifyVariant variant, ShopifyOrderLineItem lineitem)
        {
            return lineitem.ShopifyProductId == variant.ShopifyProductId &&
                   lineitem.ShopifyVariantId == variant.ShopifyVariantId;
        }
    }
}
