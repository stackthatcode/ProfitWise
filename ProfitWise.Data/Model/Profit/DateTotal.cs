using System;

namespace ProfitWise.Data.Model.Profit
{
    public class DateTotal
    {
        public DateTime OrderDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
    }
}
