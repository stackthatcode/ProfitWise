namespace ProfitWise.Data.Model
{
    public class VariantData
    {
        public string UserId { get; set; }
        public long ShopifyVariantId { get; set; }
        public long ShopifyProductId { get; set; }
        public string Sku { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }

        public ProductData ParentProduct { get; set; }
    }
}
