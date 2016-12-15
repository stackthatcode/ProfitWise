namespace ProfitWise.Data.Model
{
    public class OrderLineItemSubset
    {
        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderLineId { get; set; }

        public long? PwProductId { get; set; }
        public long? PwVariantId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
