namespace Push.Shopify.Model
{
    public class OrderLineItem
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long? VariantId { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Taxes { get; set; }
        public decimal Price { get; set; }
    }
}
