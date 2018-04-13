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

        // All of these dates are represented in Shop Timezone
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }

        public bool Cancelled => this.CancelledAt.HasValue;

        public decimal TotalRefunds => Refunds.Sum(x => x.TransactionAmount);
        public decimal TotalShippingRefund => Refunds.Sum(x => x.ShippingAdjustment);
        public decimal TotalTaxRefunds => Refunds.Sum(x => x.TaxRefundTotal);

        public decimal OrderDiscount { get; set; }

        public IList<OrderLineItem> LineItems { get; set; }
        public IEnumerable<Refund> Refunds => AllRefunds.Where(x => x.IsValid);
        public IList<Refund> AllRefunds { get; set; }

        public IEnumerable<OrderAdjustment> Adjustments => Refunds.SelectMany(x => x.OrderAdjustments);
        public IEnumerable<OrderAdjustment> NonShippingAdjustments => Adjustments.Where(x => !x.IsShippingAdjustment);
            

        public string FinancialStatus { get; set; }
        public string Tags { get; set; }
    }
}
