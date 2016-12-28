using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Reports;
using Castle.Core.Internal;

namespace ProfitWise.Data.Model.Profit
{
    public class GroupedTotal
    {
        public ReportGrouping ReportGrouping { get; set; }
        public string GroupingKey { get; set; }

        private string _groupingName;
        public string GroupingName
        {
            get
            {
                if (!_groupingName.IsNullOrEmpty())
                {
                    return _groupingName;
                }
                if (ReportGrouping == ReportGrouping.Product)
                {
                    return "(No Product Name)";
                }
                if (ReportGrouping == ReportGrouping.Variant)
                {
                    return "(No Variant Name)";
                }
                if (ReportGrouping == ReportGrouping.ProductType)
                {
                    return "(No Product Type)";
                }
                if (ReportGrouping == ReportGrouping.Vendor)
                {
                    return "(No Vendor)";
                }
                return "(No Description)";
            }
            set { _groupingName = value; }
        }
        public decimal TotalCogs { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalNumberSold { get; set; }
        public decimal AverageMargin { get; set; }
    }

    // SAVE this for Dataset2
    public static class GroupedTotalExtensions
    {
        public static List<GroupedTotal> 
                    AppendAllOthersAsDifferenceOfSummary(
                        this List<GroupedTotal> input,
                        GroupedTotal summary, 
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

        public static List<GroupedTotal> AssignGrouping(this List<GroupedTotal> input, ReportGrouping grouping)
        {
            foreach (var total in input)
            {
                total.ReportGrouping = grouping;
            }
            return input;
        }
    }
}
