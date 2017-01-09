
namespace ProfitWise.Data.Model.Profit
{
    public class ExecutiveSummary
    {
        //public int CurrencyId { get; set; }
        public int TotalQuantitySold { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
    }
}
