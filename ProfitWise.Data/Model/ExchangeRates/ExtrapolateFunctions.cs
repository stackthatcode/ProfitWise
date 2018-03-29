using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Model.ExchangeRates
{
    public static class ExtrapolateFunctions
    {
        public static List<ExchangeRate> 
                        AllConversions(DateTime date, List<ExchangeRate> rates)
        {
            if (!rates.AllSameSourceCurrency())
            {
                throw new ArgumentException("Source Currencies not all the same");
            }

            var output = new List<ExchangeRate>();

            var sourceCurrencies =
                    rates
                        .Select(x => x.DestinationCurrencyId).ToList();

            var destinationCurrencies =
                    rates
                        .Select(x => x.DestinationCurrencyId).ToList();

            foreach (var sourceCurrencyId in sourceCurrencies)
            {
                foreach (var destinationCurrencyId in destinationCurrencies)
                {
                    var rate1 =
                        rates.First(
                            x => x.DestinationCurrencyId == sourceCurrencyId).Rate;

                    var rate2 =
                        rates.First(
                            x => x.DestinationCurrencyId == destinationCurrencyId).Rate;

                    output.Add(new ExchangeRate
                    {
                        Date = date,
                        SourceCurrencyId = sourceCurrencyId,
                        DestinationCurrencyId = destinationCurrencyId,
                        Rate = 1.00m / rate1 * rate2,
                    });
                }
            }

            return output;
        }

        public static bool MissingCurrency(
                    List<ExchangeRate> rates, List<Currency> currencies)
        {
            return currencies.Any(c => MissingCurrency(rates, c.CurrencyId));
        }

        public static bool MissingCurrency(List<ExchangeRate> rates, int currencyId)
        {
            return rates.All(r => currencyId != r.DestinationCurrencyId &&
                                  currencyId != r.SourceCurrencyId);
        }

        public static List<ExchangeRate> 
                    ProjectMissingRates(
                        List<ExchangeRate> masterRates, List<ExchangeRate> target, 
                        DateTime targetDate)
        {
            var feedback = new List<ExchangeRate>();

            foreach (var masterRate in masterRates)
            {
                if (!target.Any(x => x.SourceCurrencyId == masterRate.SourceCurrencyId &&
                                    x.DestinationCurrencyId == masterRate.DestinationCurrencyId))
                {
                    var projectedRate = new ExchangeRate()
                    {
                        SourceCurrencyId = masterRate.SourceCurrencyId,
                        DestinationCurrencyId = masterRate.DestinationCurrencyId,
                        Date = targetDate,
                        Rate = masterRate.Rate,
                    };

                    target.Add(projectedRate);
                    feedback.Add(projectedRate);
                }
            }

            return feedback;
        }


        public static bool AllSameSourceCurrency(this List<ExchangeRate> rates)
        {
            if (rates.Count == 0)
            {
                throw new ArgumentException("Empty list");
            }

            var sourceCurrencyId = rates.First().SourceCurrencyId;
            return rates.All(x => x.SourceCurrencyId == sourceCurrencyId);
        }

        // Expects rates from 
        public static List<ExchangeRate> 
                        FromSourceOnly(DateTime date, List<ExchangeRate> rates)
        {
            if (!rates.AllSameSourceCurrency())
            {
                throw new ArgumentException("Source Currencies not all the same");    
            }

            List<ExchangeRate> output = new List<ExchangeRate>(rates);
            var sourceCurrencyId = rates.First().SourceCurrencyId;
            
            foreach (var rate in rates)
            {
                if (rate.DestinationCurrencyId == sourceCurrencyId)
                {
                    continue;
                }
                
                output.Add(new ExchangeRate
                {
                    Date = date,
                    SourceCurrencyId = rate.DestinationCurrencyId,
                    DestinationCurrencyId = rate.SourceCurrencyId,
                    Rate = 1.0m / rate.Rate,
                });
            }

            return output;
        }

    }
}

