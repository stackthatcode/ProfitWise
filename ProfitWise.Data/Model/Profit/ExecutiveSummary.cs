
namespace ProfitWise.Data.Model.Profit
{
    public class ExecutiveSummary
    {
        //public int CurrencyId { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal Profit { get; set; }
    }
}
