using System;
using ProfitWise.Data.Model.Profit;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.GoodsOnHand
{
    public class TotalQueryContext
    {
        public long PwShopId { get; set; }
        public long PwReportId { get; set; }

        public TotalQueryContext()
        {
        }
    }
}
