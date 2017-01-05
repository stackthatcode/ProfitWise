using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.ShopifyImport
{
    public class ShopifyOrder
    {
        public int PwShopId { get; set; }
        public long ShopifyOrderId { get; set; }
        public string Email { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderLevelDiscount { get; set; }  // We store this - comes directly from Shopify

        public IList<ShopifyOrderLineItem> LineItems { get; set; }
        public IList<ShopifyOrderAdjustment> Adjustments { get; set; }

        public void AddLineItem(ShopifyOrderLineItem lineItem)
        {
            LineItems.Add(lineItem);
            lineItem.ParentOrder = this;
        }

        // Entirely computational fields - nothing will be stored in SQL
        public decimal GrossSales => this.LineItems.Sum(x => x.GrossTotal);
        public decimal TotalDiscounts => this.LineItems.Sum(x => x.TotalDiscount);        
        public decimal TotalAdjustments => this.Adjustments.Sum(x => x.Amount);
        public decimal TotalLineItemRefunds => this.LineItems.SelectMany(x => x.Refunds).Sum(x => x.Amount);
        public decimal NetSales => GrossSales - TotalLineItemRefunds - TotalDiscounts + TotalAdjustments;


        public string FinancialStatus { get; set; }
        public string Tags { get; set;  }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public decimal NetTotal => this.LineItems.Sum(x => x.NetTotal);
        public bool Cancelled { get; set; }


        public override string ToString()
        {
            // I can't hear you, Jeremy.
            var output =
                "ShopifyOrder" + Environment.NewLine +
                $"PwShopId = {PwShopId}" + Environment.NewLine +
                $"ShopifyOrderId = {ShopifyOrderId}" + Environment.NewLine +
                $"Email = {Email}" + Environment.NewLine +
                $"OrderNumber = {OrderNumber}" + Environment.NewLine +
                $"OrderLevelDiscount = {OrderLevelDiscount}" + Environment.NewLine +
                $"CreatedAt = {CreatedAt}" + Environment.NewLine +
                $"UpdatedAt = {UpdatedAt}" + Environment.NewLine +

                $"GrossSales = {GrossSales}" + Environment.NewLine +
                $"OrderLevelDiscount = {OrderLevelDiscount}" + Environment.NewLine +
                $"TotalDiscounts = {TotalDiscounts}" + Environment.NewLine +
                $"TotalAdjustments = {TotalAdjustments}" + Environment.NewLine +
                $"TotalLineItemRefunds = {TotalLineItemRefunds}" + Environment.NewLine +
                $"NetSales = {NetSales}" + Environment.NewLine;

            foreach (var line in this.LineItems)
            {
                output += Environment.NewLine + line.ToString();
            }

            foreach (var adjustment in this.Adjustments)
            {
                output += Environment.NewLine + adjustment.ToString();
            }

            return output;
        }
    }
}

