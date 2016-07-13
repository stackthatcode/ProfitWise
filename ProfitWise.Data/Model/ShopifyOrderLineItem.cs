using System;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrderLineItem
    {
        public int ShopId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public long ShopifyOrderId { get; set; }

        public ShopifyOrder ParentOrder { get; set; }

        public long? ShopifyProductId { get; set; }
        public long? ShopifyVariantId { get; set; }
        public string Sku { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Name { get; set; }
        public long? PwProductId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalDiscount { get; set; } // Store

        public decimal TotalAfterLineItemDiscount => Quantity * UnitPrice - TotalDiscount;

        public decimal OrderDiscountAppliedToLineItem
        {
            get
            {
                if (this.ParentOrder.LineItems.Sum(x => x.TotalAfterLineItemDiscount) == 0)
                {
                    return 0m;
                }
                else
                {
                    return this.ParentOrder.OrderLevelDiscount * this.TotalAfterLineItemDiscount /
                           this.ParentOrder.LineItems.Sum(x => x.TotalAfterLineItemDiscount);
                }
            }
        }

        public decimal NetTotal => TotalAfterLineItemDiscount - OrderDiscountAppliedToLineItem;

        public decimal NetUnitPrice => NetTotal / Quantity;

        public int TotalRestockedQuantity { get; set; } // Store

        public decimal TotalRestockedValue => TotalRestockedQuantity * NetUnitPrice;

        public decimal TotalRemainingValue => NetTotal - TotalRestockedValue;

        public int RemainingQuantity => Quantity - TotalRestockedQuantity; // Store

        public decimal RestockedItemsRefundAmount
        {
            get
            {
                if (this.ParentOrder.TotalRefundAmount > this.ParentOrder.TotalRestockedValueForAllLineItems)
                {
                    return TotalRestockedValue;
                }

                if (this.ParentOrder.TotalRestockedValueForAllLineItems == 0)
                {
                    return 0m;
                }

                return this.ParentOrder.TotalRefundAmount * 
                        ( TotalRestockedValue / this.ParentOrder.TotalRestockedValueForAllLineItems );
            }
        }

        public decimal RefundAdjustment
        {
            get
            {
                if (this.ParentOrder.TotalRefundAmount <= this.ParentOrder.TotalRestockedValueForAllLineItems)
                {
                    return 0.00m;
                }

                if (this.ParentOrder.TotalRemainingValueForAllLineItems == 0m)
                {
                    return 0.00m;
                }

                return this.ParentOrder.RefundBalanceAboveRestockValue *
                           (TotalRemainingValue / this.ParentOrder.TotalRemainingValueForAllLineItems);
            }
        }

        public decimal TotalRefund => RestockedItemsRefundAmount + RefundAdjustment;

        public decimal GrossRevenue => NetTotal - TotalRefund;  // Store
    }
}
