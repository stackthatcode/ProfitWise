using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProfitWise.Data.Model.Reports;
using Castle.Core.Internal;

namespace ProfitWise.Data.Model.Profit
{
    public class GroupedTotal : IGroupedTotal
    {
        public GroupedTotal()
        {
            TotalCogs = 0m;
            TotalRevenue = 0m;
            TotalProfit = 0m;
            TotalQuantitySold = 0;
            TotalOrders = 0;
            AverageMargin = 0m;
            ProfitPercentage = 0m;
        }



        [IgnoreDataMember]
        public ReportGrouping ReportGrouping { get; set; }

        [IgnoreDataMember]
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
        public int TotalQuantitySold { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageMargin { get; set; }

        // Needs to be manually computed using the totals from the Executive Summary
        public decimal ProfitPercentage { get; set; }
    }

    public static class GroupedTotalExtensions
    {
        public static List<GroupedTotal> 
                    AppendAllOthersAsDifferenceOfSummary(
                        this List<GroupedTotal> visibleItems,
                        GroupedTotal summaryOfAllItems, 
                        string allOthersName = "All Others")
        {
            if (visibleItems.Count < 10)
            {
                return visibleItems;
            }

            if (visibleItems.Sum(x => x.TotalRevenue) == summaryOfAllItems.TotalRevenue &&
                visibleItems.Sum(x => x.TotalCogs) == summaryOfAllItems.TotalCogs &&
                visibleItems.Sum(x => x.TotalQuantitySold) == summaryOfAllItems.TotalQuantitySold)
            {
                return visibleItems;
            }

            visibleItems.Add(new GroupedTotal()
            {
                GroupingName = allOthersName,
                TotalRevenue = summaryOfAllItems.TotalRevenue - visibleItems.Sum(x => x.TotalRevenue),
                TotalCogs = summaryOfAllItems.TotalCogs - visibleItems.Sum(x => x.TotalCogs),
                TotalQuantitySold = summaryOfAllItems.TotalQuantitySold - visibleItems.Sum(x => x.TotalQuantitySold),
                TotalProfit = summaryOfAllItems.TotalProfit - visibleItems.Sum(x => x.TotalProfit),
            });
            return visibleItems;
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
