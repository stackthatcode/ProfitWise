using System;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwCogsDetail
    {
        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }
        public DateTime CogsDate { get; set; }

        public int CogsTypeId { get; set; }

        private int? _cogsCurrencyId;
        public int? CogsCurrencyId
        {
            get { return CogsTypeId == CogsType.FixedAmount ? _cogsCurrencyId : (int?)null; }
            set { _cogsCurrencyId = value; }
        }

        private decimal? _cogsAmount;
        public decimal? CogsAmount
        {
            get { return CogsTypeId == CogsType.FixedAmount ? _cogsAmount : (decimal?)null; }
            set { _cogsAmount = value; }
        }

        private decimal? _cogsPercentage;
        public decimal? CogsPercentage
        {
            get { return CogsTypeId == CogsType.MarginPercentage ? _cogsPercentage : (decimal?)null; }
            set { _cogsPercentage = value; }
        }


    }
}
