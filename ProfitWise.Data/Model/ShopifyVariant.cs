namespace ProfitWise.Data.Model
{
    public class ShopifyVariant
    {
        public int ShopId { get; set; }
        public long ShopifyVariantId { get; set; }
        public long ShopifyProductId { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }

        public ShopifyProduct ParentProduct { get; set; }
    }
}
