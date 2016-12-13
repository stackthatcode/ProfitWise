using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwReportOutput
    {
        public int CurrencyId { get; set; }

        public PwReportExecutiveSummary ExecutiveSummary { get; set; }

        public IList<PwReportKeyedSummaryTotal<long>> ProductsByMostProfitable { get; set; }
        public IList<PwReportKeyedSummaryTotal<long>> ProductsByUserSelection { get; set; }

        public IList<PwReportKeyedSummaryTotal<string>> VendorsByMostProfitable { get; set; }
        public IList<PwReportKeyedSummaryTotal<string>> VendorsByUserSelection { get; set; }

        public IList<PwReportKeyedSummaryTotal<long>> VariantByMostProfitable { get; set; }
        public IList<PwReportKeyedSummaryTotal<long>> VariantByUserSelection { get; set; }

        public IList<PwReportKeyedSummaryTotal<string>> ProductTypeByMostProfitable { get; set; }
        public IList<PwReportKeyedSummaryTotal<string>> ProductTypeByUserSelection { get; set; }
    }
}
