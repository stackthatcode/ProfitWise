using System.IO;
using System.Net;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Http
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
            try
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
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse) response;
                    _logger.Error($"Error code: {httpResponse.StatusCode}");

                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        _logger.Error(text);

                        return new HttpClientResponse()
                        {
                            StatusCode = httpResponse.StatusCode,
                            Body = text
                        };
                    }
                }
            }
        }
    }
}

