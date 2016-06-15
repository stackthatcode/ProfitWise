using System.Net;

namespace Push.Shopify.HttpClient
{
    public interface IHttpClient
    {
        HttpClientResponse ProcessRequest(HttpWebRequest request);
    }
}