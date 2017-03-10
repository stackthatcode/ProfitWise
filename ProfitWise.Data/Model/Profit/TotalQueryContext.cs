using System;
using ProfitWise.Data.Model.Reports;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Model.Profit
{
    public class TotalQueryContext
    {
        public PwShop PwShop { get; private set; }

        public long PwShopId => PwShop.PwShopId;
        public bool UseDefaultMargin => PwShop.UseDefaultMargin;
        public decimal DefaultCogsPercent => PwShop.DefaultCogsPercent;
        public int MinPaymentStatus => PwShop.MinPaymentStatus;

        public long PwReportId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReportGrouping Grouping { get; set; }
        public ColumnOrdering Ordering { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int StartingIndex => (PageNumber - 1) * PageSize;

        public bool IncludeAdjustments { get; set; }


        public TotalQueryContext(PwShop pwShop)
        {
            PwShop = pwShop;

            PageNumber = 1;
            PageSize = 10;
            Grouping = ReportGrouping.Product;
            Ordering = ColumnOrdering.ProfitDescending;
        }
    }
}
