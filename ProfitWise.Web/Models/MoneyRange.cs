namespace ProfitWise.Data.Model
{
    public class MoneyRange
    {
        public int? CurrencyId { get; set; }
        public decimal? AmountLow { get; set; }
        public decimal? AmountHigh { get; set; }
        public bool IncludesAverages { get; set; }
    }
}
