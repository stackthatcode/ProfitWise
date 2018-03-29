using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.ExchangeRates;
using ProfitWise.Data.Model.System;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    public class CurrencyService
    {
        private readonly ExchangeRateRepository _repository;
        private readonly IPushLogger _logger;
        private static List<Currency> _currencycache;
        private static Dictionary<DateTime, List<ExchangeRate>> _ratecache;


        private const int MaxDaysOfRateProjection = 30;

        private static readonly TimeSpan CacheRefreshLockInterval = new TimeSpan(4, 0, 0);

        public string BaseCurrency = "USD";

        // This sets the next allowed time to the past, thus guaraneteeing a refresh
        private static DateTime _nextRateRefreshAllowed = DateTime.UtcNow.Add(-CacheRefreshLockInterval);

        private static readonly object ExchangeRateLock = new object();
        private static readonly object CurrencyLock = new object();

        private static bool _currencyCachedLoaded;
        private static bool _rateCacheLoaded;

        static CurrencyService()
        {
            _currencyCachedLoaded = false;
            _rateCacheLoaded = false;
        }

        public CurrencyService(ExchangeRateRepository repository, IPushLogger logger)
        {
            _repository = repository;
            _logger = logger;

        }

        private List<Currency> CurrencyCache
        {
            get
            {
                if (!_currencyCachedLoaded)
                {
                    lock (CurrencyLock)
                    {
                        if (_currencycache == null)
                        {
                            _logger.Info($"Loading Currency cache");
                            _currencycache = new List<Currency>();
                            foreach (var currency in _repository.RetrieveCurrency())
                            {
                                _currencycache.Add(currency);
                            }
                            _currencyCachedLoaded = true;
                        }
                    }
                }
                return _currencycache;
            }
        }

        private Dictionary<DateTime, List<ExchangeRate>> RateCache
        {
            get
            {
                if (!_rateCacheLoaded || DateTime.UtcNow > _nextRateRefreshAllowed)
                {
                    lock (ExchangeRateLock)
                    {
                        if (!_rateCacheLoaded || DateTime.UtcNow > _nextRateRefreshAllowed)
                        {
                            LoadExchangeRateCache();
                        }
                    }
                }
                return _ratecache;
            }
        }
        
        // Cache loading function
        public void LoadExchangeRateCache()
        {
            IList<ExchangeRate> rates;

            if (_ratecache == null)
            {
                _logger.Info($"Loading Exchange Rates (ALL)");
                _ratecache = new Dictionary<DateTime, List<ExchangeRate>>();
                rates = _repository.RetrieveExchangeRates();
            }
            else
            {
                _logger.Info($"Loading Exchange Rates from ");
                var minimumDate =  _ratecache.Keys.Max().AddDays(1);
                rates = _repository.RetrieveExchangeRateFromDate(minimumDate);
            }

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
                throw new Exception("Exchange Rate cache empty - no SQL data present");
            }

            _rateCacheLoaded = true;
            _logger.Info($"Exchange Rates cached from {_ratecache.Keys.Min()} through {_ratecache.Keys.Max()}");
            _nextRateRefreshAllowed = DateTime.UtcNow.Add(CacheRefreshLockInterval);
            _logger.Info($"Next Rate Refresh allowed at: {_nextRateRefreshAllowed}");
        }
        

        // Conversion method
        public decimal Convert(decimal amount, 
                int sourceCurrencyId, int destinationCurrencyId, DateTime date)
        {
            List<ExchangeRate> rateEntry;

            if (!RateCache.Any())
            {
                throw new Exception("No Exchange Rate data present");
            }

            if (RateCache.ContainsKey(date))
            {
                rateEntry = RateCache[date];
            }
            else
            {
                var maxDate = _ratecache.Keys.Max();
                var minDate = _ratecache.Keys.Min();

                if (date > maxDate.AddDays(MaxDaysOfRateProjection))
                {
                    throw new Exception(
                        $"Exchange Rate data terminates at {maxDate} - unable to locate for {date}");
                }
                if (date < minDate.AddDays(-MaxDaysOfRateProjection))
                {
                    throw new Exception(
                        $"Exchange Rate data starts at {minDate} - unable to locate for {date}");
                }

                rateEntry = (date > maxDate) ?
                        RateCache[maxDate] :
                        RateCache[minDate];
            }

            var conversionBySource = rateEntry.First(x => x.DestinationCurrencyId == sourceCurrencyId);
            var conversionByDestination = rateEntry.First(x => x.DestinationCurrencyId == destinationCurrencyId);

            var pivotCurrencyAmount = amount / conversionBySource.Rate;
            var finalCurrencyAmount = pivotCurrencyAmount * conversionByDestination.Rate;

            return finalCurrencyAmount;
        }


        // Currency methods...
        public IList<Currency> AllCurrencies()
        {
            return CurrencyCache;
        }

        public int AbbrToCurrencyId(string abbr)
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

        public bool CurrencyExists(int currencyId)
        {
            return CurrencyCache.Exists(x => x.CurrencyId == currencyId);
        }

        public bool CurrencyExists(string abbr)
        {
            return CurrencyCache.Exists(x => x.Abbreviation == abbr);
        }
    }
}

