namespace Push.Shopify.Model
{
    public class RefundLineItem
    {
        public Refund ParentRefund { get; set; }
        public long Id { get; set; }
        public long LineItemId { get; set; }
        public int RestockQuantity { get; set; }
        public decimal TaxRefund { get; set; }
    }
}
