using System;

namespace ProfitWise.Data.Model
{
    public class PwReportOrderLineProfit
    {
        public long PwShopId { get; set; }
        public long PwMasterVariantId { get; set; }
        public long PwProductId { get; set; }
        public long PwVariantId { get; set; }
        public long ShopifyOrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }

        public int Quantity { get; set; }
        public int TotalRestockedQuantity { get; set; }
        public int NetQuantity => Quantity - TotalRestockedQuantity;
        public int UnitPrice { get; set; }
        public decimal GrossRevenue { get; set; }

        // These items will be populated by the ProfitService
        public decimal NormalizedPerUnitCogs { get; set; }
        public decimal NormalizedProfit => GrossRevenue - NetQuantity * NormalizedPerUnitCogs;
    }
}
