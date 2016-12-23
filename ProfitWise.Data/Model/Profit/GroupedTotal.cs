﻿using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Reports;


namespace ProfitWise.Data.Model.Profit
{
    public class GroupedTotal
    {
        public ReportGrouping ReportGrouping { get; set; }
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCogs;
        public int TotalNumberSold { get; set; }
    }

    // SAVE this for Dataset2
    public static class GroupedTotalExtensions
    {
        public static List<GroupedTotal> 
                    AppendAllOthersAsDifferenceOfSummary(
                        this List<GroupedTotal> input, 
                        ExecutiveSummary summary, 
                        string allOthersName = "All Others")
        {
            if (input.Count < 10)
            {
                return input;
            }
            if (input.Sum(x => x.TotalRevenue) == summary.TotalRevenue &&
                input.Sum(x => x.TotalCogs) == summary.TotalCogs &&
                input.Sum(x => x.TotalNumberSold) == summary.TotalNumberSold)
            {
                return input;
            }

            input.Add(new GroupedTotal()
            {
                GroupingName = allOthersName,
                TotalRevenue = summary.TotalRevenue - input.Sum(x => x.TotalRevenue),
                TotalCogs = summary.TotalCogs - input.Sum(x => x.TotalCogs),
                TotalNumberSold = summary.TotalNumberSold - input.Sum(x => x.TotalNumberSold)
            });
            return input;
        }
    }
}
