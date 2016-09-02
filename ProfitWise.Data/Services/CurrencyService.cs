using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    public class CurrencyService
    {
        private readonly CurrencyRepository _repository;
        private readonly IPushLogger _logger;
        private static List<Currency> _currencycache;
        private static Dictionary<DateTime, List<ExchangeRate>> _ratecache;
        

        public CurrencyService(CurrencyRepository repository, IPushLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private List<Currency> CurrencyCache
        {
            get
            {
                if (_currencycache == null)
                {
                    _logger.Info($"Loading Currency cache");
                    _currencycache = new List<Currency>();
                    foreach (var currency in _repository.RetrieveCurrency())
                    {
                        _currencycache.Add(currency);
                    }
                }
                return _currencycache;
            }
        }

        private Dictionary<DateTime, List<ExchangeRate>> RateCache
        {
            get
            {
                if (_ratecache == null)
                {
                    _ratecache = new Dictionary<DateTime, List<ExchangeRate>>();
                    LoadExchangeRateCache();
                }
                return _ratecache;
            }
        }
        
        public void LoadExchangeRateCache(DateTime? minimumDate = null)
        {
            _logger.Info($"Loading Exchange Rates cache from {minimumDate}");

            var rates =
                minimumDate.HasValue
                    ? _repository.RetrieveExchangeRates(minimumDate.Value)
                    : _repository.RetrieveExchangeRates();

            foreach (var rate in rates)
            {
                if (!_ratecache.ContainsKey(rate.Date))
                {
                    _ratecache[rate.Date] = new List<ExchangeRate>();
                }
                _ratecache[rate.Date].Add(rate);
            }

            if (!_ratecache.Any())
            {
                _logger.Info("Exchange Rate cache empty - no SQL data present");
            }
            else
            { 
                _logger.Info(
                    $"Exchange Rates cached from {_ratecache.Keys.Min()} through {_ratecache.Keys.Max()}");
            }
        }

        public DateTime? LatestExchangeRateDate => 
            RateCache.Keys.Count == 0 ? (DateTime?)null : RateCache.Keys.Max();

        //private void VerifyAndRefreshRateCache(DateTime date)
        //{
        //    if (!RateCache.ContainsKey(date))
        //    {
        //        _logger.Info($"Exchange Rates refresh triggered for {date}");
        //        var minimumDate = _ratecache.Keys.Max().AddDays(1);
        //        LoadExchangeRates(minimumDate);
        //    }
        //}

        public decimal Convert(decimal amount, int sourceCurrencyId, int destinationCurrencyId, DateTime date)
        {
            //VerifyAndRefreshRateCache(date);

            var rateEntry = 
                !RateCache.ContainsKey(date)
                    ? RateCache[_ratecache.Keys.Max()]
                    : RateCache[date];

            var conversionBySource = rateEntry.First(x => x.DestinationCurrencyId == sourceCurrencyId);
            var conversionByDestination = rateEntry.First(x => x.DestinationCurrencyId == destinationCurrencyId);

            var pivotCurrencyAmount = amount / conversionBySource.Rate;
            var finalCurrencyAmount = pivotCurrencyAmount * conversionByDestination.Rate;

            return finalCurrencyAmount;
        }

        public IList<Currency> AllCurrencies()
        {
            return CurrencyCache;
        }

        public int AbbreviationToCurrencyId(string abbr)
        {
            var currency = CurrencyCache.FirstOrDefault(x => x.Abbreviation == abbr);
            if (currency == null)
            {
                throw new ArgumentException($"Cannot find Currency data for Abbreviation: {abbr}");
            }
            return currency.CurrencyId;
        }

        public string CurrencyIdToAbbreviation(int currencyId)
        {
            var currency = CurrencyCache.FirstOrDefault(x => x.CurrencyId == currencyId);
            if (currency == null)
            {
                throw new ArgumentException($"Cannot find Currency data for CurrencyId: {currencyId}");
            }
            return currency.Abbreviation;
        }
    }
}
