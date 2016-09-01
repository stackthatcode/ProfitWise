namespace ProfitWise.Data.Model
{
    public class OrderLineItemToProfitWiseMapping
    {
        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderLineId { get; set; }

        public long? PwProductId { get; set; }
        public long? PwVariantId { get; set; }
    }
}
