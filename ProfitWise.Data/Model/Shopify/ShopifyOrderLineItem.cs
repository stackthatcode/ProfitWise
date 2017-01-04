using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Shopify
{
    public class ShopifyOrderLineItem
    {
        public int PwShopId { get; set; }
        public long ShopifyOrderLineId { get; set; }
        public long ShopifyOrderId { get; set; }

        public ShopifyOrder ParentOrder { get; set; }
        public IList<ShopifyOrderLineRefund> Refunds { get; set; }

        public DateTime OrderDateTimestamp { get; set; }
        public DateTime OrderDate { get; set; }

        public long? PwProductId { get; set; }
        public long? PwVariantId { get; set; }

        public int Quantity { get; set; }   // From Shopify - we store this
        public decimal UnitPrice { get; set; }  // From Shopify - we store this
        public decimal LineDiscount { get; set; }   // From Shopify - we store this

        public decimal GrossLineAmount => Quantity * UnitPrice;

        public decimal LineAmountAfterLineDiscount => GrossLineAmount - LineDiscount;
        // Our computation is exactly like Shopify's: each line item will have deducted from it an amount
        // ... in proportion to its Line Amount after line-level Discount * the total Order Discount

        public decimal PortionOfOrderDiscount
        {
            get
            {
                if (this.ParentOrder.LineItems.Sum(x => x.LineAmountAfterLineDiscount) == 0)
                {
                    return 0m;
                }
                else
                {
                    return this.ParentOrder.OrderLevelDiscount * this.LineAmountAfterLineDiscount /
                           this.ParentOrder.LineItems.Sum(x => x.LineAmountAfterLineDiscount);
                }
            }
        }
        public decimal LineAmountAfterAllDiscounts => LineAmountAfterLineDiscount - PortionOfOrderDiscount;

        public decimal DiscountedUnitPrice => LineAmountAfterAllDiscounts / Quantity;        

        public decimal TotalRefund { get; set; } // Get from Child Refunds
        public decimal NetTotal => LineAmountAfterAllDiscounts - TotalRefund;


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
                $"TotalDiscount = {LineDiscount}" + Environment.NewLine +
                $"LineAmountAfterLineDiscount = {LineAmountAfterLineDiscount}" + Environment.NewLine +
                $"PortionOfOrderDiscount = {PortionOfOrderDiscount}" + Environment.NewLine +
                $"NetUnitPrice = {DiscountedUnitPrice}" + Environment.NewLine +
                $"NetTotal = {LineAmountAfterAllDiscounts}" + Environment.NewLine +
                $"TotalRefund = {TotalRefund}" + Environment.NewLine +
                $"NetTotal = {NetTotal}" + Environment.NewLine;
        }
    }
}
