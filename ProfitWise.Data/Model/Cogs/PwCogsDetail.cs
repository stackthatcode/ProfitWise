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
        public decimal? CogsMarginPercent { get; set; }

        public decimal CogsPercentOfUnitPrice => 100m - CogsMarginPercent ?? 0m;


        public PwCogsDetail Clone(Action<PwCogsDetail> constraint = null)
        {
            var output = new PwCogsDetail
            {
                PwShopId = this.PwShopId,
                PwMasterVariantId = this.PwMasterVariantId,
                PwMasterProductId = this.PwMasterProductId,

                CogsDate = this.CogsDate,
                CogsTypeId = this.CogsTypeId,
                CogsAmount = this.CogsAmount,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsMarginPercent = this.CogsMarginPercent,
            };

            constraint?.Invoke(output);
            return output;
        }

        public PwCogsDetail AttachKeys(long pwShopId, long? pwMasterVariantId, long? pwMasterProductId)
        {
            this.PwShopId = pwShopId;
            this.PwMasterVariantId = pwMasterVariantId;
            this.PwMasterProductId = pwMasterProductId;
            return this;
        }        
    }
}

