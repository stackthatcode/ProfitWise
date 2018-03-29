using System;

namespace ProfitWise.Data.Model.ExchangeRates
{
    public class ExchangeRate
    {
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }

        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }
}
