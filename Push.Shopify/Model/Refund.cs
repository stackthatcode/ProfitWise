using System.Collections.Generic;

namespace Push.Shopify.Model
{
    public class Refund
    {
        public long Id { get; set; }
        public decimal TransactionAmount { get; set; }
        public IList<RefundLineItem> LineItems { get; set; }
        public Order ParentOrder { get; set; }
    }
}
