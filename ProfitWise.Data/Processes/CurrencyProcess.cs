using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.ExchangeRateApis;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Processes
{
    public class CurrencyProcess
    {
        private readonly CurrencyService _currencyService;
        private readonly SystemStateRepository _systemStateRepository;
        private readonly CurrencyRepository _currencyRepository;
        private readonly FixerApiRepository _fixerApiRepository;
        private readonly IPushLogger _pushLogger;


        // *** TODO - update to 2006-01-01
        public readonly DateTime ExchangeRateStartDate = new DateTime(2014, 01, 01);
        public const int ProfitWiseBaseCurrency = 1; // Corresponds to USD

        public CurrencyProcess(
                CurrencyService currencyService,
                SystemStateRepository systemStateRepository,
                CurrencyRepository currencyRepository,
                FixerApiRepository fixerApiRepository,
                IPushLogger pushLogger)
        {
            _currencyService = currencyService;
            _systemStateRepository = systemStateRepository;
            _currencyRepository = currencyRepository;
            _fixerApiRepository = fixerApiRepository;
            _pushLogger = pushLogger;
        }

        public void Execute()
        {
            var systemState = _systemStateRepository.Retrieve();

            // Note: pay attention to null-coalescing...
            var startDate = systemState.ExchangeRateLastDate?.AddDays(1) ?? ExchangeRateStartDate;
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

            // ** additional padding to save from when the System is missing Exchange Rate data
            var paddingDate = endDate.AddDays(1);
            var lastRates = _currencyRepository.RetrieveExchangeRateByDate(endDate);

            var endDatePadding = endDate.AddDays(30);
            _pushLogger.Info($"Inserting Exchange Rate padding from {endDate} through {endDatePadding}");
            while (paddingDate <= endDatePadding)
            {
                WriteRatesForDate(paddingDate, lastRates);
                paddingDate = paddingDate.AddDays(1);
            }
        }

        private void WriteRatesForDate(DateTime date, List<ExchangeRate> rates)
        {
            using (var trans = _currencyRepository.InitiateTransaction())
            {
                // Clear out the date
                _currencyRepository.DeleteForDate(date);
                _pushLogger.Debug($"Writing Exchange Rates for {date}");

                foreach (var rate in ExtrapolateFullSetOfExchangeRate(date, rates))
                {
                    _currencyRepository.InsertExchangeRate(
                        new ExchangeRate()
                        {
                            Date = date,
                            SourceCurrencyId = rate.SourceCurrencyId,
                            DestinationCurrencyId = rate.DestinationCurrencyId,
                            Rate = rate.Rate,
                        });
                }
                
                trans.Commit();
            }
        }

        private List<ExchangeRate> 
                ExtrapolateFullSetOfExchangeRate(DateTime date, List<ExchangeRate> canonicalListOfRates)
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
