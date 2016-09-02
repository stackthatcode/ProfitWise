using System.Net;
using Push.Foundation.Web.Http;

namespace ProfitWise.Data.ExchangeRateApis
{
    public class FixerApiRequestFactory
    {
        private readonly HttpClientFacadeConfig _configuration;
        private const string BaseUrl = "http://api.fixer.io/";


        public FixerApiRequestFactory(HttpClientFacadeConfig configuration)
        {
            _configuration = configuration;
        }

        public HttpWebRequest HttpGet(string path)
        {
            var request = FactoryWorker("GET", path);
            return request;
        }

        private HttpWebRequest FactoryWorker(string httpVerb, string path)
        {
            var url = BaseUrl + path;
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _configuration.Timeout;
            req.Method = httpVerb;
            return req;
        }
    }
}

