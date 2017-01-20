using System;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwCogsDetail
    {
        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }
        public DateTime CogsDate { get; set; }

        public int CogsTypeId { get; set; }
        public int CogsCurrencyId { get; set; }
        public decimal CogsAmount { get; set; }
        public decimal? CogsPercentage { get; set; }
    }
}
