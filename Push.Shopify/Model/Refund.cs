using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Model
{
    public class Refund
    {
        public long Id { get; set; }
        public Order ParentOrder { get; set; }
        public IList<RefundLineItem> LineItems { get; set; }
        public List<OrderAdjustment> OrderAdjustments { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public decimal TransactionAmount => Transactions.Sum(x => x.Amount);

        public decimal ShippingAdjustment 
                        => OrderAdjustments
                            .Where(x => x.IsShippingAdjustment)
                            .Sum(x => x.Amount);

        public decimal NonShippingAdjustment
                        => OrderAdjustments
                            .Where(x => !x.IsShippingAdjustment)
                            .Sum(x => x.Amount);

        public decimal TaxRefundTotal => LineItems.Sum(x => x.TaxTotal);
    }
}
