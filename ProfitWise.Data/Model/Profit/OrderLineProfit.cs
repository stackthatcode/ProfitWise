using System;

namespace ProfitWise.Data.Model
{
    public class OrderLineProfit
    {
        public long PwMasterVariantId { get; set; }

        public PwReportSearchStub SearchStub { get; set; }

        public long PwShopId { get; set; }
        public long PwProductId { get; set; }   // These technically are not needed
        public long PwVariantId { get; set; }    // These technically are not needed

        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public long ShopifyOrderId { get; set; }
        public long ShopifyOrderLineId { get; set; }

        public int Quantity { get; set; }
        public int TotalRestockedQuantity { get; set; }
        public int NetQuantity => Quantity - TotalRestockedQuantity;
        public int UnitPrice { get; set; }
        public decimal GrossRevenue { get; set; }

        // These items will be populated by the ProfitService
        public decimal PerUnitCogs { get; set; }
        public decimal TotalCogs => NetQuantity * PerUnitCogs;
        public decimal Profit => GrossRevenue - TotalCogs;
    }
}
