using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProfitWise.Data.Model.Preferences;

namespace ProfitWise.Data.Model.Reports
{

    public static class SystemReportFactory
    {
        public const long OverallProfitabilityId = 1;
        public const long GoodsOnHandId = 2;

        public static string CustomDefaultNameBuilder(int reportNumber)
        {
            return $"Untitled Report-{reportNumber}";
        }

        public static bool IsSystemReport(this long reportId)
        {
            return reportId == OverallProfitabilityId || reportId == GoodsOnHandId;
        }

        public static PwReport OverallProfitability(DateRange dateRange)
        {
            return new PwReport
            {
                PwReportId = OverallProfitabilityId,
                OriginalReportId = OverallProfitabilityId,
                ReportTypeId = ReportType.Profitability,

                Name = "Overall Profitability",
                StartDate = dateRange.StartDate,
                EndDate = dateRange.EndDate,
                GroupingId = ReportGrouping.Overall,
                OrderingId = ReportOrdering.ProfitabilityDescending,
                IsSystemReport = true,
            };
        }

        public static PwReport GoodsOnHandReport(DateTime today)
        {
            return new PwReport
            {
                PwReportId = GoodsOnHandId,
                OriginalReportId = GoodsOnHandId,
                ReportTypeId = ReportType.GoodsOnHand,

                Name = "Inventory Valuation",
                StartDate = today,
                EndDate = today,
                GroupingId = ReportGrouping.ProductType,
                OrderingId = ReportOrdering.ProfitabilityDescending,
                IsSystemReport = true,
            };
        }
    }
}
