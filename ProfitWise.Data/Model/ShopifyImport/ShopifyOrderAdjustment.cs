using System;
using ProfitWise.Data.Model.ShopifyImport;

namespace ProfitWise.Data.Model.Shopify
{
    public class ShopifyOrderAdjustment
    {
        public int PwShopId { get; set; }
        public long ShopifyAdjustmentId { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public long ShopifyOrderId { get; set; }
        public ShopifyOrder Order { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Kind { get; set; }
        public string Reason { get; set; }
    }
}

