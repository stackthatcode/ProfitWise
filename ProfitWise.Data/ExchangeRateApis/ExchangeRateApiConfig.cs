using Push.Foundation.Web.Http;

namespace ProfitWise.Data.ExchangeRateApis
{
    public class ExchangeRateApiConfig : HttpClientFacadeConfig
    {
        public string OerApiKey { get; set; }
        public string FixerApiKey { get; set; }


        public ExchangeRateApiConfig()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 666;

            // These will be more often than not be set at an instance-level
            ThrowExceptionOnBadHttpStatusCode = false;
            RetriesEnabled = true;

            FixerApiKey = "";
            OerApiKey = "";
        }
    }
}
