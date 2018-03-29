using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Hangfire;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.ExchangeRates;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Processes
{
    public class ExchangeRateProcess
    {
        private readonly CurrencyService _currencyService;
        private readonly ExchangeRateRepository _exchangeRateRepository;
        private readonly OerApiRepository _rateHttpRepository;
        private readonly IPushLogger _pushLogger;
        private readonly SystemRepository _systemRepository;
        
        public readonly DateTime DefaultStartDateOfDataset = new DateTime(2006, 01, 01);
        public const int ProfitWiseBaseCurrency = 1; // Corresponds to USD
        public const int FutureProjectionDays = 30;


        public ExchangeRateProcess(
                CurrencyService currencyService,
                ExchangeRateRepository exchangeRateRepository,
                OerApiRepository rateHttpRepository,
                IPushLogger pushLogger,
                SystemRepository systemRepository)
        {
            _currencyService = currencyService;
            _exchangeRateRepository = exchangeRateRepository;
            _rateHttpRepository = rateHttpRepository;
            _pushLogger = pushLogger;
            _systemRepository = systemRepository;
        }


        [Queue(ProfitWiseQueues.ExchangeRateRefresh)]
        public void Execute()
        {
            var lastDate = _systemRepository.RetrieveLastExchangeRateDate();
            var startDate = lastDate?.AddDays(1) ?? DefaultStartDateOfDataset;
            var endDate = DateTime.UtcNow.Date;
             
            if (startDate > endDate)
            {
                _pushLogger.Info("Aborting Exchange Rate import - all imported data is current");
                return;
            }
            
            _pushLogger.Info(
                $"Importing Exchange Rates from OpenExchangeRate API from {startDate} to {endDate}");

            var date = startDate;
            var baseCurrency = _currencyService.CurrencyIdToAbbreviation(ProfitWiseBaseCurrency);
            var allCurrencies = 
                _currencyService
                        .AllCurrencies()
                        .Select(x => x.Abbreviation)
                        .ToList();
            
            List<ExchangeRate> lastRates = null;

            while (date <= endDate)
            {
                _pushLogger.Debug($"Importing Exchange Rates for {date}");
                var rates = _rateHttpRepository
                                .GetHistoricalRates(date, baseCurrency, allCurrencies);

                if (lastRates != null)
                {
                    var feedback = 
                        ExtrapolateFunctions
                            .ProjectMissingRates(lastRates, rates, date);

                    foreach (var item in feedback)
                    {
                        _pushLogger.Warn(
                            $"Missing data for Destination Currency Id {item.DestinationCurrencyId} on {date}");
                    }
                }

                lastRates = rates;

                _pushLogger.Info($"Writing Exchange Rates for {date}");

                var extrapolatedRates = ExtrapolateFunctions.AllConversions(date, rates);
                WriteRatesForDate(date, extrapolatedRates);
                _systemRepository.UpdateLastExchangeRateDate(date);

                date = date.AddDays(1);
            }            

            if (lastRates == null)
            {
                return;
            }

            // Projects Rates into future
            var projectionDate = endDate.AddDays(1);
            while (projectionDate <= endDate.AddDays(FutureProjectionDays))
            {
                _pushLogger.Info($"Writing (projected) Exchange Rates for {projectionDate}");

                var extrapolatedRates = ExtrapolateFunctions.AllConversions(projectionDate, lastRates);
                WriteRatesForDate(projectionDate, extrapolatedRates);
                projectionDate = projectionDate.AddDays(1);
            }
        }

        // Loads a single Currency from default Min Date to Today
        public void LoadNewCurrency(string symbol, DateTime minDate)
        {
            var currencies =
                    _currencyService
                        .AllCurrencies()
                        .Select(x => x.Abbreviation)
                        .ToList();

            if (currencies.All(x => x != symbol))
            {
                throw new ArgumentException(
                    $"Currency {symbol} not loaded into 'currency' table");
            }

            var baseCurrency = symbol;
            var baseCurrencyId = _currencyService.AbbrToCurrencyId(baseCurrency);

            var maxDate = 
                _systemRepository.RetrieveLastExchangeRateDate() ?? DateTime.UtcNow.Date;
            
            var currentDate = minDate;
            var lastRatesFromApi = new List<ExchangeRate>();

            while (currentDate <= maxDate)
            {
                var ratesFromApi =
                    _rateHttpRepository
                        .GetHistoricalRates(currentDate, baseCurrency, currencies);

                if (lastRatesFromApi != null)
                {
                    var feedback =
                        ExtrapolateFunctions
                            .ProjectMissingRates(lastRatesFromApi, ratesFromApi, currentDate);

                    foreach (var item in feedback)
                    {
                        _pushLogger.Warn(
                            $"Missing data for Destination Currency Id " +
                            $"{item.DestinationCurrencyId} on {currentDate}");
                    }
                }

                lastRatesFromApi = ratesFromApi;

                var extrapolatedRates = 
                        ExtrapolateFunctions.FromSourceOnly(currentDate, ratesFromApi);

                WriteRatesForSingleCurrency(currentDate, baseCurrencyId, extrapolatedRates);

                currentDate = currentDate.AddDays(1);
            }
        }

        private void WriteRatesForDate(DateTime date, List<ExchangeRate> rates)
        {
            using (var trans = new TransactionScope())
            {
                _exchangeRateRepository.DeleteForDate(date);

                foreach (var rate in rates)
                {
                    _exchangeRateRepository.InsertExchangeRate(
                            new ExchangeRate()
                            {
                                Date = date,
                                SourceCurrencyId = rate.SourceCurrencyId,
                                DestinationCurrencyId = rate.DestinationCurrencyId,
                                Rate = rate.Rate,
                            });
                }

                trans.Complete();
            }
        }

        private void WriteRatesForSingleCurrency(
                DateTime date, int baseCurrencyId, List<ExchangeRate> rates) 
        {
            using (var trans = new TransactionScope())
            {
                // Clear out the date
                _exchangeRateRepository.DeleteForDateAndCurrency(date, baseCurrencyId);
                
                foreach (var rate in rates)
                {
                    _exchangeRateRepository.InsertExchangeRate(
                            new ExchangeRate()
                            {
                                Date = date,
                                SourceCurrencyId = rate.SourceCurrencyId,
                                DestinationCurrencyId = rate.DestinationCurrencyId,
                                Rate = rate.Rate,
                            });
                }

                trans.Complete();
            }
        }

    }
}

