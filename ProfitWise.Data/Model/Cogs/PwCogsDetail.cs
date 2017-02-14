using System;

namespace ProfitWise.Data.Model.Cogs
{
    // Object that maps specifically to the profitwisemastervariantcogsdetail table in SQL
    // For updates, the services/repositories are using Cogs Dto as a "data envelope"
    public class PwCogsDetail
    {
        public long PwShopId { get; set; }
        public long? PwMasterVariantId { get; set; } 
        public DateTime CogsDate { get; set; }

        public int CogsTypeId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public decimal? CogsMarginPercent { get; set; }
        public decimal CogsPercentOfUnitPrice => 100m - CogsMarginPercent ?? 0m;
        
        public PwCogsDetail AttachMasterVariantId(long? pwMasterVariantId)
        {
            this.PwMasterVariantId = pwMasterVariantId;
            return this;
        }

        public PwCogsDetail Clone()
        {
            var output = new PwCogsDetail
            {
                PwShopId = this.PwShopId,
                PwMasterVariantId = this.PwMasterVariantId,
                
                CogsDate = this.CogsDate,
                CogsTypeId = this.CogsTypeId,
                CogsAmount = this.CogsAmount,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsMarginPercent = this.CogsMarginPercent,
            };

            return output;
        }

        // The CogsDto is used now for Cost of Goods computations, whereas PwCogsDetail is purely SQL storage
        public CogsDto ToCogsDto()
        {
            return new CogsDto()
            {
                CogsDate = this.CogsDate,
                CogsTypeId = this.CogsTypeId,
                CogsAmount = this.CogsAmount,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsMarginPercent = this.CogsMarginPercent,
            };
        }
    }
}

