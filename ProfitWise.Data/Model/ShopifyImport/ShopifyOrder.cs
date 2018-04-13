﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.ShopifyImport
{
    public class ShopifyOrder
    {
        public long PwShopId { get; set; }
        public long ShopifyOrderId { get; set; }
        public string Email { get; set; }
        public string OrderNumber { get; set; }
        public decimal OrderLevelDiscount { get; set; }  // We store this - comes directly from Shopify
        public DateTime OrderDate { get; set; }

        public IList<ShopifyOrderLineItem> LineItems { get; set; }
        public IList<ShopifyOrderAdjustment> Adjustments { get; set; }

        public void AddLineItem(ShopifyOrderLineItem lineItem)
        {
            LineItems.Add(lineItem);
            lineItem.ParentOrder = this;
        }

        // Entirely computational fields - nothing will be stored in SQL
        public decimal GrossSales => this.LineItems.Sum(x => x.GrossTotal);
        public decimal NetTotal => this.LineItems.Sum(x => x.NetTotal);

        public decimal TotalDiscounts => this.LineItems.Sum(x => x.TotalDiscount);        
        public decimal TotalAdjustments => this.Adjustments.Sum(x => x.Amount);
        public decimal TotalLineItemRefunds => this.LineItems.SelectMany(x => x.Refunds).Sum(x => x.Amount);
        public decimal NetSales => GrossSales - TotalLineItemRefunds - TotalDiscounts + TotalAdjustments;

        public decimal BalancingCorrection => NetSales < 0 ? -NetSales : 0m;

        public DateTime LastActivityDate
        {
            get
            {
                var dates = new List<DateTime>() {this.OrderDate};
                dates.AddRange(this.LineItems.SelectMany(x => x.Refunds).Select(x => x.RefundDate));
                dates.AddRange(this.Adjustments.Select(x => x.AdjustmentDate));
                return dates.Max();
            }
        }

        public int FinancialStatus { get; set; }
        public string Tags { get; set;  }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public bool Cancelled { get; set; }


        public override string ToString()
        {
            var output =
                "ShopifyOrder" + Environment.NewLine +
                $"PwShopId = {PwShopId}" + Environment.NewLine +
                $"ShopifyOrderId = {ShopifyOrderId}" + Environment.NewLine +
                $"Email = {Email}" + Environment.NewLine +
                $"OrderNumber = {OrderNumber}" + Environment.NewLine +
                $"OrderDate = {OrderDate}" + Environment.NewLine +
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

