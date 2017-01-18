using System;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwCogsDetail
    {
        public long PwCogsDetailId { get; set; }
        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }

        public DateTime CogsDate { get; set; }
        public CogsType CogsTypeId { get; set; }
        public int CogsCurrencyId { get; set; }
        public decimal CogsAmount { get; set; }
    }
}
