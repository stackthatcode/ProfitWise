using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrder
    {
        public int ShopId { get; set; }
        public long ShopifyOrderId { get; set; }
        public string Email { get; set; }
        public string OrderNumber { get; set;  }
        public decimal OrderLevelDiscount { get; set;  }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IList<ShopifyOrderLineItem> LineItems { get; set; }

        public decimal TotalPrice { get; set; }
        
        public decimal TotalRefundAmount { get; set; }  // Comes from Shopify + store it

        public decimal TotalRestockedValueForAllLineItems => this.LineItems.Sum(x => x.TotalRestockedValue);

        public decimal TotalRemainingValueForAllLineItems => this.LineItems.Sum(x => x.TotalRemainingValue);

        public decimal RefundBalanceAboveRestockValue => TotalRefundAmount -
                                                         LineItems.Sum(x => x.RestockedItemsRefundAmount);

        public decimal TotalGrossRevenue => this.LineItems.Sum(x => x.GrossRevenue);
    }
}

