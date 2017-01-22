using System;

namespace ProfitWise.Data.Model.Cogs
{

    // Largely a pass-thru context with some additional properties that will be used to for the SQL update
    public class CogsUpdateOrderContext
    {
        public PwCogsDetail Cogs { get; set; }

        public long PwShopId => Cogs.PwShopId;
        public long? PwMasterProductId => Cogs.PwMasterProductId;
        public long? PwMasterVariantId => Cogs.PwMasterVariantId;

        public int CogsTypeId => Cogs.CogsTypeId;
        public int? CogsCurrencyId => Cogs.CogsCurrencyId;
        public decimal? CogsAmount => Cogs.CogsAmount;
        public decimal CogsPercentOfUnitPrice => Cogs.CogsPercentOfUnitPrice;

        public int DestinationCurrencyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
