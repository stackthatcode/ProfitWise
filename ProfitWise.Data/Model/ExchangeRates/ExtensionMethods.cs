using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Model.ExchangeRates
{
    public static class ExtensionMethods
    {
        public static List<string> ToIsoAbbreviation(this IList<Currency> currencies)
        {
            return currencies.Select(x => x.Abbreviation).ToList();
        }
    }
}
