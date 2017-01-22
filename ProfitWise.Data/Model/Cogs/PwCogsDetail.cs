using System;

namespace ProfitWise.Data.Model.Cogs
{
    public class PwCogsDetail
    {
        public long? PwMasterVariantId { get; set; }
        public long? PwMasterProductId { get; set; }
        public long PwShopId { get; set; }
        public DateTime CogsDate { get; set; }

        public int CogsTypeId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public decimal? CogsPercentage { get; set; }

        public PwCogsDetail CloneWithConstraints(Action<PwCogsDetail> constraint)
        {
            var output = new PwCogsDetail
            {
                PwMasterVariantId = this.PwMasterVariantId,
                PwMasterProductId = this.PwMasterProductId,

                PwShopId = this.PwShopId,
                CogsDate = this.CogsDate,
                CogsTypeId = this.CogsTypeId,
                CogsAmount = this.CogsAmount,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsPercentage = this.CogsPercentage,
            };

            constraint(output);
            return output;
        }

        public PwCogsDetail AttachKeys(long? pwMasterVariantId, long? pwMasterProductId)
        {
            this.PwMasterVariantId = pwMasterVariantId;
            this.PwMasterProductId = pwMasterProductId;
            return this;
        }
    }
}
