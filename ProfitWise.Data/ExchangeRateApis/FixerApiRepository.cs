using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Model;
using ProfitWise.Data.Services;
using Push.Foundation.Web.Http;

namespace ProfitWise.Data.ExchangeRateApis
{
    public class FixerApiRepository
    {
        private readonly FixerApiRequestFactory _factory;
        private readonly IHttpClientFacade _httpClient;
        private readonly CurrencyService _service;

        public FixerApiRepository(
                FixerApiRequestFactory factory, FixerApiConfig config, IHttpClientFacade httpClient, CurrencyService service)
        {
            _factory = factory;
            _httpClient = httpClient;
            _httpClient.Configuration = config;
            _service = service;
        }

        public List<ExchangeRate> RetrieveConversion(DateTime date, string baseAbbreviation)
        {
            return RetrieveConversion(
                    date, baseAbbreviation,
                    _service.AllCurrencies().Select(x => x.Abbreviation).ToList());
        }

        public List<ExchangeRate> RetrieveConversion(
                            DateTime date, string baseAbbreviation, IList<string> targetAbbreviations)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var targetAbbreviationsFormatted = String.Join(",", targetAbbreviations);
            var url = formattedDate + 
                "?base=" + baseAbbreviation + "&symbols=" + targetAbbreviationsFormatted;

            var request = _factory.HttpGet(url);
            var response = _httpClient.ExecuteRequest(request);
            dynamic root = JsonConvert.DeserializeObject(response.Body);

            var output = new List<ExchangeRate>();

            foreach (var abbreviation in targetAbbreviations)
            {
                decimal? conversionMultiplier = root.rates[abbreviation];

                if (abbreviation == baseAbbreviation)
                {
                    output.Add(new ExchangeRate
                    {
                        Date = date,
                        SourceCurrencyId = _service.AbbreviationToCurrencyId(baseAbbreviation),
                        DestinationCurrencyId = _service.AbbreviationToCurrencyId(abbreviation),
                        Rate = 1.0m,
                    });
                    continue;
                }

                if (conversionMultiplier != null)
                {
                    output.Add(new ExchangeRate
                    {
                        Date = date,
                        SourceCurrencyId = _service.AbbreviationToCurrencyId(baseAbbreviation),
                        DestinationCurrencyId = _service.AbbreviationToCurrencyId(abbreviation),
                        Rate = conversionMultiplier.Value,
                    });
                }
            }
            return output;
        }

    }
}
