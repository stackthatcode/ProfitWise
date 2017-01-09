namespace Push.Shopify.Model
{
    public class OrderAdjustment
    {
        public Refund Refund { get; set; }

        public long Id { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Kind { get; set; }
        public string Reason { get; set; }

        public bool IsShippingAdjustment => Kind == "shipping_refund";
    }
}
