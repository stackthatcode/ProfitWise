using System.IO;
using System.Net;
using Push.Foundation.Utilities.Logging;

namespace Push.Shopify.HttpClient
{
    public class HttpClient : IHttpClient
    {
        private readonly IPushLogger _logger;

        public HttpClient(IPushLogger logger)
        {
            _logger = logger;
        }

        public virtual HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            using (HttpWebResponse resp = (HttpWebResponse) request.GetResponse())
            {
                var sr = new StreamReader(resp.GetResponseStream());
                var messageResponse = sr.ReadToEnd();
                
                return new HttpClientResponse
                {
                    StatusCode = resp.StatusCode,
                    Body = messageResponse,
                };
            }
        }
    }
}

