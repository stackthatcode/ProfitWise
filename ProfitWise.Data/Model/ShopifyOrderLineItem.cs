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
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Name { get; set; }
        public long? PwProductId { get; set; }
    }
}
