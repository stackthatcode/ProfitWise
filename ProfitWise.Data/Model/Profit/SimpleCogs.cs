using System;

namespace ProfitWise.Data.Model
{
    public class PwReportMasterVariantCogs
    {
        public long PwMasterVariantId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }

        public bool HasData => CogsCurrencyId != null && CogsCurrencyId != 0 && CogsAmount != null;
    }
}

