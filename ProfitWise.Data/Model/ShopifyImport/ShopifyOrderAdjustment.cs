using System;

namespace ProfitWise.Data.Model.ShopifyImport
{
    public class ShopifyOrderAdjustment
    {
        public int PwShopId { get; set; }
        public long? ShopifyAdjustmentId { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public long ShopifyOrderId { get; set; }
        public ShopifyOrder Order { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Kind { get; set; }
        public string Reason { get; set; }


        public override string ToString()
        {
            return
                "ShopifyOrderAdjustment" + Environment.NewLine +
                $"PwShopId = {PwShopId}" + Environment.NewLine +
                $"ShopifyAdjustmentId = {ShopifyAdjustmentId}" + Environment.NewLine +
                $"ShopifyOrderId = {ShopifyOrderId}" + Environment.NewLine +
                $"AdjustmentDate = {AdjustmentDate}" + Environment.NewLine +
                $"Amount = {Amount}" + Environment.NewLine +
                $"TaxAmount = {TaxAmount}" + Environment.NewLine +
                $"Kind = {Kind}" + Environment.NewLine +                
                $"Reason = {Reason}" + Environment.NewLine;
        }
    }
}

