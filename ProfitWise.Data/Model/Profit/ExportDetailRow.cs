using System.Runtime.Serialization;

namespace ProfitWise.Data.Model.Profit
{
    public class ExportDetailRow
    {
        [IgnoreDataMember]
        public int PwVariantId { get; set; }
        public string Vendor { get; set; }
        public string ProductType { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set; }

        public int TotalQuantitySold { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit { get; set; }

        public decimal AverageMarginPerUnitSold { get; set; }
        public decimal UnitPriceAverage { get; set; }
        public decimal UnitCogsAverage { get; set; }

        public decimal CurrentUnitPrice { get; set; }
        public decimal CurrentUnitCogs { get; set; }
        public decimal CurrentMargin { get; set; }

        [IgnoreDataMember]
        public bool StockedDirectly { get; set; }
        public string IsStockedDirectly => StockedDirectly ? "Yes" : "No";

        // Needs to be manually computed
        public decimal ProfitPercentage { get; set; }
    }
}
