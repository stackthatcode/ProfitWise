﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.ShopifyImport
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
        //public bool Cancelled => ParentOrder.Cancelled;

        public long? PwProductId { get; set; }
        public long? PwVariantId { get; set; }

        public int Quantity { get; set; }           // From Shopify - we store this
        public int NetQuantity => Quantity - Refunds.Sum(x => x.RestockQuantity);   // We store this

        public decimal UnitPrice { get; set; }      // From Shopify - we store this
        public decimal GrossTotal => Quantity * UnitPrice;
        public decimal LineDiscount { get; set; }   // From Shopify - we store this


        // Our computation is exactly like Shopify's: each line item will have deducted from it an amount
        // ... in proportion to its Line Amount after line-level Discount * the total Order Discount
        public decimal TotalAfterLineDiscount => GrossTotal - LineDiscount;        
        public decimal PortionOfOrderDiscount
        {
            get
            {
                if (this.ParentOrder.LineItems.Sum(x => x.TotalAfterLineDiscount) == 0)
                {
                    return 0m;
                }
                else
                {
                    return Math.Round(this.ParentOrder.OrderLevelDiscount * this.TotalAfterLineDiscount /
                           this.ParentOrder.LineItems.Sum(x => x.TotalAfterLineDiscount), 2);
                }
            }
        }
        public decimal TotalDiscount => LineDiscount + PortionOfOrderDiscount;
        public decimal TotalAfterAllDiscounts => GrossTotal - TotalDiscount;    // We store this
         
        // Additional computation fields that aren't stored - we dump them to the interface later for forensics
        public decimal DiscountedUnitPrice => TotalAfterAllDiscounts / Quantity;
        public decimal TotalRefund => Refunds.Sum(x => x.Amount);
        public decimal NetTotal => TotalAfterAllDiscounts - TotalRefund;
        
        public decimal? UnitCogs { get; set; }

        public void SetProfitWiseVariant(PwVariant pwVariant)
        {
            this.PwProductId = pwVariant.PwProductId;
            this.PwVariantId = pwVariant.PwVariantId;

            foreach (var refund in this.Refunds)
            {
                refund.PwProductId = pwVariant.PwProductId;
                refund.PwVariantId = pwVariant.PwVariantId;
            }
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
                $"GrossTotal = {GrossTotal}" + Environment.NewLine +

                $"TotalDiscount = {LineDiscount}" + Environment.NewLine +
                $"TotalAfterLineDiscount = {TotalAfterLineDiscount}" + Environment.NewLine +
                $"PortionOfOrderDiscount = {PortionOfOrderDiscount}" + Environment.NewLine +
                $"TotalDiscount = {TotalDiscount}" + Environment.NewLine +
                $"TotalAfterAllDiscounts = {TotalAfterAllDiscounts}" + Environment.NewLine +
                $"TotalAfterAllDiscounts = {TotalAfterAllDiscounts}" + Environment.NewLine +
                $"DiscountedUnitPrice = {DiscountedUnitPrice}" + Environment.NewLine +

                $"TotalRefund = {TotalRefund}" + Environment.NewLine +
                $"NetTotal = {NetTotal}" + Environment.NewLine;
        }
    }
}