using System;
using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Model
{
    public class Order
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal TotalTax { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public decimal TotalRefunds => Refunds.Sum(x => x.RefundTotal);

        public decimal TotalTaxRefunds => Refunds.Sum(x => x.TaxRefundTotal);

        public decimal OrderDiscount { get; set; }

        public IList<OrderLineItem> LineItems { get; set; }
        public IList<Refund> Refunds { get; set; }
    }
}
