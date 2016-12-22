using System;
using System.Collections.Generic;

namespace ProfitWise.Data.Model.Profit
{
    public class DateTotal
    {
        public DateTime OrderDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
    }

    public static class DateTotalExtensions
    {
        public static decimal Total(
                this Dictionary<DateTime, DateTotal> input, 
                DateTime start, DateTime end, Func<DateTotal, decimal> property)
        {
            var total = 0m;
            var current = start;
            while (current <= end)
            {
                if (input.ContainsKey(current))
                {
                    total += input[current].TotalProfit;
                }
                current = current.AddDays(1);
            }
            return total;
        }
    }
}
