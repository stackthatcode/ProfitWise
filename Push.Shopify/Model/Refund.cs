using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Model
{
    public class Refund
    {
        public long Id { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal ShippingAdjustment { get; set; }
        public decimal RefundTotal => TransactionAmount + ShippingAdjustment;
        public decimal TaxRefundTotal => LineItems.Sum(x => x.TaxRefund);

        public IList<RefundLineItem> LineItems { get; set; }
        public Order ParentOrder { get; set; }
    }
}
