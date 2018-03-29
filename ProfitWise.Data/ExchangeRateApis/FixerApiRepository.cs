using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Model.ExchangeRates;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using static System.String;

namespace ProfitWise.Data.ExchangeRateApis
{
    public class FixerApiRepository
    {
        private readonly FixerApiRequestFactory _factory;
        private readonly IHttpClientFacade _httpClient;
        private readonly IPushLogger _logger;
        private readonly CurrencyService _service;
        private readonly string _fixerApiKey;

        public FixerApiRepository(
                FixerApiRequestFactory factory, 
                ExchangeRateApiConfig config, 
                IHttpClientFacade httpClient, 
                IPushLogger logger,
                CurrencyService service)
        {
            _factory = factory;
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.Configuration = config;
            _service = service;
            _fixerApiKey = ConfigurationManager.AppSettings["FixerApiKey"];
        }

        public List<ExchangeRate> GetHistoricalRates(DateTime date, string baseCurrency)
        {
            var allCurrencies = 
                _service.AllCurrencies()
                        .Select(x => x.Abbreviation)
                        .ToList();

            return GetHistoricalRates(date, baseCurrency, allCurrencies);
        }

        public List<ExchangeRate> GetHistoricalRates(
                    DateTime date, string baseCurrency, IList<string> targetCurrencies)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var targetCurrencyFlat = Join(",", targetCurrencies);

            var queryString =
                new QueryStringBuilder()
                    .Add("access_key", _fixerApiKey)
                    .Add("base", baseCurrency)
                    .Add("symbols", targetCurrencyFlat)
                    .ToString();

            var url = $"{formattedDate}?{queryString}";
            var request = _factory.HttpGet(url);

            _logger.Info($"Invoking Fixer API {url}");
            var response = _httpClient.ExecuteRequest(request);

            _logger.Info($"Result: {response.Body}");
            dynamic root = JsonConvert.DeserializeObject(response.Body);

            var output = new List<ExchangeRate>();

            foreach (var abbreviation in targetCurrencies)
            {
                var conversionMultiplier = root.rates[abbreviation];
                if (conversionMultiplier == null)
                {
                    continue;
                }
                var rate = abbreviation == baseCurrency ? 1.0m : conversionMultiplier;

                output.Add(new ExchangeRate
                {
                    Date = date,
                    SourceCurrencyId = _service.AbbrToCurrencyId(baseCurrency),
                    DestinationCurrencyId = _service.AbbrToCurrencyId(abbreviation),
                    Rate = rate,
                });                
            }

            return output;
        }
        
        public string GetTimeSeries(
                DateTime startDate, DateTime endDate, string baseCurrency, 
                IList<string> targetAbbreviations)
        {
            var standardizedStart = startDate.ToString("yyyy-MM-dd");
            var standardizedEnd = endDate.ToString("yyyy-MM-dd");
            var targetCurrency = Join(",", targetAbbreviations);

            var queryString =
                new QueryStringBuilder()
                    .Add("access_key", _fixerApiKey)
                    .Add("base", baseCurrency)
                    .Add("symbols", targetCurrency)
                    .Add("start_date", standardizedStart)
                    .Add("end_date", standardizedEnd)
                    .ToString();

            var url = $"timeseries?{queryString}";

            var request = _factory.HttpGet(url);
            var response = _httpClient.ExecuteRequest(request);
            
            var output = new List<ExchangeRate>();
            
            //dynamic root = JsonConvert.DeserializeObject(response.Body);

            //foreach (var abbreviation in targetAbbreviations)
            //{
            //    decimal? conversionMultiplier = root.rates[abbreviation];

            //    if (abbreviation == baseAbbreviation)
            //    {
            //        output.Add(new ExchangeRate
            //        {
            //            Date = date,
            //            SourceCurrencyId = _service.AbbreviationToCurrencyId(baseAbbreviation),
            //            DestinationCurrencyId = _service.AbbreviationToCurrencyId(abbreviation),
            //            Rate = 1.0m,
            //        });
            //        continue;
            //    }

            //    if (conversionMultiplier != null)
            //    {
            //        output.Add(new ExchangeRate
            //        {
            //            Date = date,
            //            SourceCurrencyId = _service.AbbreviationToCurrencyId(baseAbbreviation),
            //            DestinationCurrencyId = _service.AbbreviationToCurrencyId(abbreviation),
            //            Rate = conversionMultiplier.Value,
            //        });
            //    }
            //}
            return response.Body;
        }


    }
}
