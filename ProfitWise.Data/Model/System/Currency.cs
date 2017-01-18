namespace ProfitWise.Data.Model.System
{
    public class Currency
    {
        public const int DefaultCurrencyId = 1; // USD

        public int CurrencyId { get; set; }
        public string Abbreviation { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
    }
}
