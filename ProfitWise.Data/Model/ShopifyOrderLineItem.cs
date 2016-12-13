using System;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrderLineItem
    {
        public int PwShopId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public long ShopifyOrderId { get; set; }

        public ShopifyOrder ParentOrder { get; set; }
        public DateTime OrderDate { get; set; }

        public long? PwProductId { get; set; }
        public long? PwVariantId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalDiscount { get; set; } // Store
        public decimal TotalMinusLineItemDiscount => Quantity * UnitPrice - TotalDiscount;

        // Hey! Our computation is like Shopify's!
        public decimal ProportionOfOrderDiscount
        {
            get
            {
                if (this.ParentOrder.LineItems.Sum(x => x.TotalMinusLineItemDiscount) == 0)
                {
                    return 0m;
                }
                else
                {
                    return this.ParentOrder.OrderLevelDiscount * this.TotalMinusLineItemDiscount /
                           this.ParentOrder.LineItems.Sum(x => x.TotalMinusLineItemDiscount);
                }
            }
        }

        // Why is this NetTotal 
        public decimal NetTotal => TotalMinusLineItemDiscount - ProportionOfOrderDiscount;

        public decimal NetUnitPrice => NetTotal / Quantity;

        
        public int TotalRestockedQuantity { get; set; } // Store
        

        public decimal TotalRestockedValue => TotalRestockedQuantity * NetUnitPrice;

        public decimal TotalRemainingValue => NetTotal - TotalRestockedValue;

        public int RemainingQuantity => Quantity - TotalRestockedQuantity;

        public decimal RestockedItemsRefundAmount
        {
            get
            {
                if (this.ParentOrder.TotalRefundExcludingTaxAndShipping > this.ParentOrder.TotalRestockedValueForAllLineItems)
                {
                    return TotalRestockedValue;
                }

                if (this.ParentOrder.TotalRestockedValueForAllLineItems == 0)
                {
                    return 0m;
                }

                return this.ParentOrder.TotalRefundExcludingTaxAndShipping * 
                        ( TotalRestockedValue / this.ParentOrder.TotalRestockedValueForAllLineItems );
            }
        }

        public decimal OrderLevelRefundAdjustment
        {
            get
            {
                if (this.ParentOrder.TotalRefundExcludingTaxAndShipping <= this.ParentOrder.TotalRestockedValueForAllLineItems)
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

        public decimal TotalRefund => RestockedItemsRefundAmount + OrderLevelRefundAdjustment;

        public decimal GrossRevenue
        {
            get { return NetTotal - TotalRefund; }
            set {  }
        }



        public override string ToString()
        {
            return
                "ShopifyOrderLineItem" + Environment.NewLine +
                $"PwShopId = {PwShopId}" + Environment.NewLine +
                $"ShopifyOrderLineId = {ShopifyOrderLineId}" + Environment.NewLine +
                $"ShopifyOrderId = {ShopifyOrderId}" + Environment.NewLine +
                $"PwProductId = {PwProductId}" + Environment.NewLine +
                $"Quantity = {Quantity}" + Environment.NewLine +
                $"UnitPrice = {UnitPrice}" + Environment.NewLine +
                $"TotalDiscount = {TotalDiscount}" + Environment.NewLine +
                $"TotalMinusLineItemDiscount = {TotalMinusLineItemDiscount}" + Environment.NewLine +
                $"ProportionOfOrderDiscount = {ProportionOfOrderDiscount}" + Environment.NewLine +
                $"NetTotal = {NetTotal}" + Environment.NewLine +
                $"NetUnitPrice = {NetUnitPrice}" + Environment.NewLine +

                $"TotalRestockedQuantity = {TotalRestockedQuantity}" + Environment.NewLine +
                $"TotalRestockedValue = {TotalRestockedValue}" + Environment.NewLine +
                $"TotalRemainingValue = {TotalRemainingValue}" + Environment.NewLine +
                $"RemainingQuantity = {RemainingQuantity}" + Environment.NewLine +
                $"RestockedItemsRefundAmount = {RestockedItemsRefundAmount}" + Environment.NewLine +
                $"RefundAdjustment = {OrderLevelRefundAdjustment}" + Environment.NewLine +
                $"TotalRefund = {TotalRefund}" + Environment.NewLine +
                $"GrossRevenue = {GrossRevenue}" + Environment.NewLine;
        }

    }
}
