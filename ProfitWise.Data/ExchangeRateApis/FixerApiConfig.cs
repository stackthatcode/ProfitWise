using System.Threading;
using Push.Foundation.Web.Http;

namespace ProfitWise.Data.ExchangeRateApis
{
    public class FixerApiConfig : HttpClientFacadeConfig
    {
        public FixerApiConfig()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 0;

            // These will be more often than not be set at an instance-level
            ThrowExceptionOnBadHttpStatusCode = false;
            RetriesEnabled = true;
        }
    }
}
