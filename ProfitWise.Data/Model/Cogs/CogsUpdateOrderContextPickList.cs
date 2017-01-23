namespace ProfitWise.Data.Model.Cogs
{
    // Largely a pass-thru context with some additional properties that will be used to for the SQL update
    public class CogsUpdateOrderContextPickList
    {
        public long PwShopId => Cogs.PwShopId;
        public long? PwPickListId { get; set; }

        public PwCogsDetail Cogs { get; set; }

        public int CogsTypeId => Cogs.CogsTypeId;
        public int? CogsCurrencyId => Cogs.CogsCurrencyId;
        public decimal? CogsAmount => Cogs.CogsAmount;
        public decimal CogsPercentOfUnitPrice => Cogs.CogsPercentOfUnitPrice;

        public int DestinationCurrencyId { get; set; }
    }
}
