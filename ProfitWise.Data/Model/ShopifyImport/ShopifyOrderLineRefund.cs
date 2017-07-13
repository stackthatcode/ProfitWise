using System;

namespace ProfitWise.Data.Model.ShopifyImport
{
    public class ShopifyOrderLineRefund
    {
        public long PwShopId { get; set; }
        public long ShopifyRefundId { get; set; }

        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public ShopifyOrderLineItem OrderLineItem { get; set; }

        public long? PwProductId => OrderLineItem.PwProductId;
        public long? PwVariantId => OrderLineItem.PwVariantId;

        public DateTime RefundDate { get; set; }
        public decimal Amount { get; set; }
        public int RestockQuantity { get; set; }
    }
}
