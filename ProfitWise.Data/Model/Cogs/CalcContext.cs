namespace ProfitWise.Data.Model.Cogs
{
    public class CalcContext
    {
        public decimal PercentMultiplier { get; set; }
        public decimal FixedAmount { get; set; }
        public int SourceCurrencyId { get; set; }
    }
}
