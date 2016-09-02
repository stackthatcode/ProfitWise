using System.Net;

namespace Push.Foundation.Web.Http
{
    public interface IHttpClient
    {
        HttpClientResponse ProcessRequest(HttpWebRequest request);
    }
}