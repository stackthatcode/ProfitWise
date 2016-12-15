using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwReportSummary
    {
        public int CurrencyId { get; set; }
        public PwReportExecutiveSummary ExecutiveSummary { get; set; }

        public IList<ReportGroupedTotal> ProductsByMostProfitable { get; set; }
        public IList<ReportGroupedTotal> VendorsByMostProfitable { get; set; }
        public IList<ReportGroupedTotal> VariantByMostProfitable { get; set; }
        public IList<ReportGroupedTotal> ProductTypeByMostProfitable { get; set; }
    }
}
