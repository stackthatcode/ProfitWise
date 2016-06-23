using System;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrderLineItem
    {
        public int ShopId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public long ShopifyOrderId { get; set; }
        public long? ShopifyProductId { get; set; }
        public long? ShopifyVariantId { get; set; }
        public string ReportedSku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalDiscount { get; set; }
    }
}
