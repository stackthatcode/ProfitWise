using System;
using System.IO;
using System.Net;

namespace Push.Shopify.HttpClient
{
    public class HttpClient : IHttpClient
    {
        public HttpClientResponse ProcessRequest(HttpWebRequest request)
        {
            using (HttpWebResponse resp = (HttpWebResponse) request.GetResponse())
            {
                var sr = new StreamReader(resp.GetResponseStream());

                return new HttpClientResponse()
                {
                    StatusCode = resp.StatusCode,
                    Body = sr.ReadToEnd()
                };
            }
        }
    }
}