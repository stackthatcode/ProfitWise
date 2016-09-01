using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Services
{
    public class CurrencyConversionService
    {
        private readonly CurrencyRepository _repository;

        private static Dictionary<DateTime, List<Currency>> _currencycache;
        private static Dictionary<DateTime, List<CurrencyConversion>> _conversioncache;
        

        public CurrencyConversionService(CurrencyRepository repository)
        {
            _repository = repository;
        }

        private Dictionary<DateTime, List<CurrencyConversion>> ConversionCache
        {
            get
            {
                if (_conversioncache == null)
                {
                    _conversioncache = new Dictionary<DateTime, List<CurrencyConversion>>();
                    foreach (var conversion in _repository.RetrieveCurrencyConversions())
                    {
                        if (!_conversioncache.ContainsKey(conversion.Date))
                        {
                            _conversioncache[conversion.Date] = new List<CurrencyConversion>();
                        }
                        _conversioncache[conversion.Date].Add(conversion);
                    }

                }
                return _conversioncache;
            }
        }

        public decimal Convert(decimal amount, int sourceCurrencyId, int destinationCurrencyId, DateTime date)
        {
            // TODO - get Cache Entry by Date parameter... temporarily, always get 9/1/2016
            var conversionTable = ConversionCache[new DateTime(2016, 9, 1)];

            var conversionBySource = conversionTable.First(x => x.DestinationCurrencyId == sourceCurrencyId);
            var conversionByDestination = conversionTable.First(x => x.DestinationCurrencyId == destinationCurrencyId);

            var pivotCurrencyAmount = amount / conversionBySource.Multipler;
            var finalCurrencyAmount = pivotCurrencyAmount * conversionByDestination.Multipler;

            return finalCurrencyAmount;
        }

    }
}
