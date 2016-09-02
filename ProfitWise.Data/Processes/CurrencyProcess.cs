using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly CurrencyRepository _currencyRepository;
        private readonly FixerApiRepository _fixerApiRepository;
        private readonly IPushLogger _pushLogger;

        public readonly DateTime DateMinimum = new DateTime(2006, 01, 01);
        public const int ProfitWiseBaseCurrency = 1; // Corresponds to USD

        public CurrencyProcess(
                CurrencyService currencyService, 
                CurrencyRepository currencyRepository,
                FixerApiRepository fixerApiRepository,
                IPushLogger pushLogger)
        {
            _currencyService = currencyService;
            _currencyRepository = currencyRepository;
            _fixerApiRepository = fixerApiRepository;
            _pushLogger = pushLogger;
        }

        public void Execute()
        {
            var startDate = _currencyService.LatestExchangeRateDate == null
                ? DateMinimum
                : _currencyService.LatestExchangeRateDate.Value.AddDays(1);
            var endDate = DateTime.Today;
            var date = startDate;

            if (startDate > endDate)
            {
                _pushLogger.Info("Aborting Exchange Rate update - all data is current");
                return;
            }

            var baseCurrency = _currencyService.CurrencyIdToAbbreviation(ProfitWiseBaseCurrency);
            _pushLogger.Info($"Importing Exchange Rates from FixerApi from {startDate} to {endDate}");

            while (date <= endDate)
            {
                _pushLogger.Debug($"Importing Exchange Rates for {date}");

                var rates = _fixerApiRepository.RetrieveConversion(date, baseCurrency);

                using (var trans = _currencyRepository.InitiateTransaction())
                {
                    foreach (var rate in rates)
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

                    date = date.AddDays(1);
                    trans.Commit();
                }
            }
        }
    }
}
