using System;
using System.Net;
using Push.Shopify.HttpClient;

namespace ProfitWise.Batch.CodeDump
{

    public class HttpClientThatThrowsUnauthorized : IHttpClient
    {
        private IHttpClient _httpClient;
        Random rnd = new Random();

        public HttpClientThatThrowsUnauthorized()
        {
            _httpClient = new HttpClient(null);
        }

        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            int pick = rnd.Next(1, 5);

            if (pick == 4)
            {
                return new HttpClientResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                };
            }

            return _httpClient.ProcessRequest(request);
        }
    }


    public class HttpClientThatThrowsErrors : IHttpClient
    {
        private IHttpClient _httpClient;
        Random rnd = new Random();

        public HttpClientThatThrowsErrors()
        {
            _httpClient = new HttpClient(null);
        }

        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            int pick = rnd.Next(1, 5);

            if (pick > 3)
            {
                throw new Exception("System fault - network crash");
            }

            return _httpClient.ProcessRequest(request);
        }
    }
}
