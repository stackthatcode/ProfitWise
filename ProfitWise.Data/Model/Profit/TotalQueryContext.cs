using System;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.Profit
{
    public class TotalQueryContext
    {
        public long PwShopId { get; set; }
        public long PwReportId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReportGrouping Grouping { get; set; }
        public ColumnOrdering Ordering { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int StartingIndex => (PageNumber - 1) * PageSize;

        public TotalQueryContext()
        {
            PageNumber = 1;
            PageSize = 10;
            Grouping = ReportGrouping.Product;
            Ordering = ColumnOrdering.ProfitDescending;
        }
    }
}
