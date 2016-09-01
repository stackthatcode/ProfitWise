using System;

namespace ProfitWise.Data.Model
{
    public class CurrencyConversion
    {
        public long SourceCurrencyId { get; set; }
        public long DestinationCurrencyId { get; set; }

        public DateTime Date { get; set; }
        public decimal Multipler { get; set; }
    }
}
