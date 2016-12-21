using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Shopify;

namespace ProfitWise.Data.Model
{
    public class ShopifyOrder
    {
        public int PwShopId { get; set; }
        public long ShopifyOrderId { get; set; }
        public string Email { get; set; }
        public string OrderNumber { get; set; }
        
        public IList<ShopifyOrderLineItem> LineItems { get; set; }

        public void AddLineItem(ShopifyOrderLineItem lineItem)
        {
            LineItems.Add(lineItem);
            lineItem.ParentOrder = this;
        }

        public decimal OrderLevelDiscount { get; set; }
        public decimal SubTotal { get; set; }

        public decimal TotalRefund { get; set; }
        public decimal TaxRefundAmount { get; set; }
        public decimal ShippingRefundAmount { get; set; }

        public string FinancialStatus { get; set; }
        public string Tags { get; set;  }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }



        public decimal TotalRefundExcludingTaxAndShipping => TotalRefund - TaxRefundAmount + ShippingRefundAmount;

        public decimal TotalRestockedValueForAllLineItems => this.LineItems.Sum(x => x.TotalRestockedValue);

        public decimal TotalRemainingValueForAllLineItems => this.LineItems.Sum(x => x.TotalRemainingValue);

        public decimal RefundBalanceAboveRestockValue => TotalRefundExcludingTaxAndShipping -
                                                         LineItems.Sum(x => x.RestockedItemsRefundAmount);

        public decimal TotalGrossRevenue => this.LineItems.Sum(x => x.GrossRevenue);
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
                $"TotalPrice = {SubTotal}" + Environment.NewLine +
                $"TotalRefund = {TotalRefund}" + Environment.NewLine +
                $"TaxRefundAmount = {TaxRefundAmount}" + Environment.NewLine +
                $"ShippingRefundAmount = {ShippingRefundAmount}" + Environment.NewLine +
                $"TotalRefundExcludingTaxAndShipping = {TotalRefundExcludingTaxAndShipping}" + Environment.NewLine +
                $"TotalRestockedValueForAllLineItems = {TotalRestockedValueForAllLineItems}" + Environment.NewLine +
                $"TotalRemainingValueForAllLineItems = {TotalRemainingValueForAllLineItems}" + Environment.NewLine +
                $"RefundBalanceAboveRestockValue = {RefundBalanceAboveRestockValue}" + Environment.NewLine +
                $"TotalGrossRevenue = {TotalGrossRevenue}" + Environment.NewLine;

            foreach (var line in this.LineItems)
            {
                output += Environment.NewLine + line.ToString();
            }

            return output;
        }
    }
}

