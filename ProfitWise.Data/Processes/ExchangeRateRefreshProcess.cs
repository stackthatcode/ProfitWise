using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Hangfire;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Processes
{
    public class ExchangeRateRefreshProcess
    {
        private readonly CurrencyService _currencyService;
        private readonly ExchangeRateRepository _exchangeRateRepository;
        private readonly FixerApiRepository _fixerApiRepository;
        private readonly IPushLogger _pushLogger;


        public readonly DateTime DefaultStartDateOfDataset = new DateTime(2006, 01, 01);
        public const int ProfitWiseBaseCurrency = 1; // Corresponds to USD


        public ExchangeRateRefreshProcess(
                CurrencyService currencyService,
                ExchangeRateRepository exchangeRateRepository,
                FixerApiRepository fixerApiRepository,
                IPushLogger pushLogger)
        {
            _currencyService = currencyService;
            _exchangeRateRepository = exchangeRateRepository;
            _fixerApiRepository = fixerApiRepository;
            _pushLogger = pushLogger;
        }


        [Queue(Queues.ExchangeRateRefresh)]
        public void Execute()
        {
            var lastDate = _exchangeRateRepository.LatestExchangeRateDate();
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

                while (date <= endDate)
                {
                    _pushLogger.Debug($"Importing Exchange Rates for {date}");

                    var rates = _fixerApiRepository.RetrieveConversion(date, baseCurrency);

                    WriteRatesForDate(date, rates);
                    date = date.AddDays(1);
                }
            }            
        }

        private void WriteRatesForDate(DateTime date, List<ExchangeRate> rates)
        {
            using (var trans = new TransactionScope())
            {
                // Clear out the date
                _exchangeRateRepository.DeleteForDate(date);
                _pushLogger.Info($"Writing Exchange Rates for {date}");

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
