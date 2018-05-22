namespace ProfitWise.Data.Model.Profit
{
    public class ExportDetailRow
    {
        public ExportDetailRow()
        {
            TotalCogs = 0m;
            TotalRevenue = 0m;
            TotalProfit = 0m;
            TotalQuantitySold = 0;
            TotalOrders = 0;
            AverageMargin = 0m;
            ProfitPercentage = 0m;
        }

        public string Vendor { get; set; }
        public string ProductType { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set; }


        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalQuantitySold { get; set; }
        public int TotalOrders { get; set; }
       
        public decimal UnitPriceAverage { get; set; }
        public decimal UnitCogsAverage { get; set; }
        public decimal UnitMarginAverage { get; set; }

        public decimal AverageMargin { get; set; }
        
        // Needs to be manually computed
        public decimal ProfitPercentage { get; set; }
    }
}
