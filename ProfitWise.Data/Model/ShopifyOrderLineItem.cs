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

        public DateTime OrderDate { get; set; }

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

        // Hey! Our computation is like Shopify's!
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
                $"ShopId = {ShopId}" + Environment.NewLine +
                $"ShopifyOrderLineId = {ShopifyOrderLineId}" + Environment.NewLine +
                $"ShopifyOrderId = {ShopifyOrderId}" + Environment.NewLine +
                $"ShopifyProductId = {ShopifyProductId}" + Environment.NewLine +
                $"ShopifyVariantId = {ShopifyVariantId}" + Environment.NewLine +
                $"Sku = {Sku}" + Environment.NewLine +
                $"ProductTitle = {ProductTitle}" + Environment.NewLine +
                $"VariantTitle = {VariantTitle}" + Environment.NewLine +
                $"Name = {Name}" + Environment.NewLine +
                $"PwProductId = {PwProductId}" + Environment.NewLine +
                $"Quantity = {Quantity}" + Environment.NewLine +
                $"UnitPrice = {UnitPrice}" + Environment.NewLine +
                $"TotalDiscount = {TotalDiscount}" + Environment.NewLine +
                $"TotalAfterLineItemDiscount = {TotalAfterLineItemDiscount}" + Environment.NewLine +
                $"OrderDiscountAppliedToLineItem = {OrderDiscountAppliedToLineItem}" + Environment.NewLine +
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
