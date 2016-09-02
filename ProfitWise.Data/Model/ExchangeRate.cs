using System;

namespace ProfitWise.Data.Model
{
    public class ExchangeRate
    {
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }

        public DateTime Date { get; set; }
        public decimal Rate { get; set; }
    }
}
