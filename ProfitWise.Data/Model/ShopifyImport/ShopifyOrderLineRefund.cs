using System;
using ProfitWise.Data.Model.ShopifyImport;

namespace ProfitWise.Data.Model.Shopify
{
    public class ShopifyOrderLineRefund
    {
        public int PwShopId { get; set; }
        public long ShopifyRefundId { get; set; }

        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public ShopifyOrderLineItem OrderLineItem { get; set; }

        public long PwProductId { get; set; }
        public long PwVariantId { get; set; }

        public DateTime RefundDate { get; set; }
        public decimal Amount { get; set; }
    }
}
