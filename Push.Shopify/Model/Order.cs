﻿using System;
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

        // Coming from Shopify, these include the TimeZone of the Shop
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public bool Cancelled => this.CancelledAt.HasValue;

        public decimal TotalRefunds => Refunds.Sum(x => x.TransactionAmount);
        public decimal TotalShippingRefund => Refunds.Sum(x => x.ShippingAdjustment);
        public decimal TotalTaxRefunds => Refunds.Sum(x => x.TaxRefundTotal);

        public decimal OrderDiscount { get; set; }

        public IList<OrderLineItem> LineItems { get; set; }
        public IList<Refund> Refunds { get; set; }

        public IEnumerable<OrderAdjustment> Adjustments => Refunds.SelectMany(x => x.OrderAdjustments);
        public IEnumerable<OrderAdjustment> NonShippingAdjustments => Adjustments.Where(x => !x.IsShippingAdjustment);
            

        public string FinancialStatus { get; set; }
        public string Tags { get; set; }
    }
}
