using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Hangfire;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;


namespace ProfitWise.Data.Processes
{
    public class ExchangeRateRefreshProcess
    {
        private readonly CurrencyService _currencyService;
        private readonly ExchangeRateRepository _exchangeRateRepository;
        private readonly FixerApiRepository _fixerApiRepository;
        private readonly BatchLogger _pushLogger;
        private readonly SystemRepository _systemRepository;


        public readonly DateTime DefaultStartDateOfDataset = new DateTime(2006, 01, 01);
        public const int ProfitWiseBaseCurrency = 1; // Corresponds to USD

        public const int FutureProjectionDays = 30;


        public ExchangeRateRefreshProcess(
                CurrencyService currencyService,
                ExchangeRateRepository exchangeRateRepository,
                FixerApiRepository fixerApiRepository,
                BatchLogger pushLogger,
                SystemRepository systemRepository)
        {
            _currencyService = currencyService;
            _exchangeRateRepository = exchangeRateRepository;
            _fixerApiRepository = fixerApiRepository;
            _pushLogger = pushLogger;
            _systemRepository = systemRepository;
        }


        [Queue(ProfitWiseQueues.ExchangeRateRefresh)]
        public void Execute()
        {
            var lastDate = _systemRepository.RetrieveLastExchangeRateDate();
            var startDate = lastDate?.AddDays(1) ?? DefaultStartDateOfDataset;
            var endDate = DateTime.Today;
             
            if (startDate > endDate)
            {
                _pushLogger.Info("Aborting Exchange Rate import - all imported data is current");
            }
            else
            {
                _pushLogger.Info($"Importing Exchange Rates from FixerApi from {startDate} to {endDate}");

                var date = startDate;
                var baseCurrency = _currencyService.CurrencyIdToAbbreviation(ProfitWiseBaseCurrency);

                List<ExchangeRate> lastRates = null;

                while (date <= endDate)
                {
                    _pushLogger.Debug($"Importing Exchange Rates for {date}");
                    var rates = _fixerApiRepository.RetrieveConversion(date, baseCurrency);
                    lastRates = rates;

                    _pushLogger.Info($"Writing Exchange Rates for {date}");
                    WriteRatesForDate(date, rates);
                    date = date.AddDays(1);
                }

                _systemRepository.UpdateLastExchangeRateDate(endDate);
                if (lastRates == null)
                {
                    return;
                }

                var projectionDate = endDate.AddDays(1);
                while (projectionDate <= endDate.AddDays(FutureProjectionDays))
                {
                    _pushLogger.Info($"Writing (projected) Exchange Rates for {projectionDate}");
                    WriteRatesForDate(projectionDate, lastRates);
                    projectionDate = projectionDate.AddDays(1);
                }
            }            
        }

        private void WriteRatesForDate(DateTime date, List<ExchangeRate> rates)
        {
            using (var trans = new TransactionScope())
            {
                // Clear out the date
                _exchangeRateRepository.DeleteForDate(date);

                foreach (var rate in ExtrapolateFullSetOfExchangeRate(date, rates))
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

        private List<ExchangeRate> ExtrapolateFullSetOfExchangeRate(DateTime date, List<ExchangeRate> canonicalListOfRates)
        {
            var output = new List<ExchangeRate>();
            var sourceCurrencies = canonicalListOfRates.Select(x => x.DestinationCurrencyId).ToList();
            var destinationCurrencies = canonicalListOfRates.Select(x => x.DestinationCurrencyId).ToList();

            foreach (var sourceCurrencyId in sourceCurrencies)
            {
                foreach (var destinationCurrencyId in destinationCurrencies)
                {
                    var rate1 = 
                        canonicalListOfRates.First(
                            x => x.DestinationCurrencyId == sourceCurrencyId).Rate;

                    var rate2 = 
                        canonicalListOfRates.First(
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
    }
}
