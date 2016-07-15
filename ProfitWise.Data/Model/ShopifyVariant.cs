namespace ProfitWise.Data.Model
{
    public class ShopifyVariant
    {
        public int ShopId { get; set; }
        public long ShopifyVariantId { get; set; }
        public long ShopifyProductId { get; set; }
        public long? PwProductId { get; set; }

        public ShopifyProduct ParentProduct { get; set; }
    }
}
