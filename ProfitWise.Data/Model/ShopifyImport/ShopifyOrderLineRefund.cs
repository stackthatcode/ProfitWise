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

        private decimal _amount = 0m;
        public decimal Amount
        {
            get { return OrderLineItem.IsGiftCard ? 0 : _amount; }
            set { _amount = value; }
        }
        public int RestockQuantity { get; set; }
    }
}
