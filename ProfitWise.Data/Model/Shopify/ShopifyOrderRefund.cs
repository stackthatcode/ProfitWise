using System;

namespace ProfitWise.Data.Model.Shopify
{
    public class ShopifyOrderRefund
    {
        public int PwShopId { get; set; }
        public long ShopifyRefundId { get; set; }
        public long ShopifyOrderId { get; set; }
        public ShopifyOrder Order { get; set; }

        public DateTime RefundDate { get; set; }
        public decimal Amount { get; set; }
    }
}
