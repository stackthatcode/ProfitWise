using System.Net;

namespace Push.Foundation.Web.Http
{
    public interface IHttpClientFacade
    {
        HttpClientResponse ExecuteRequest(HttpWebRequest request);
        HttpClientFacadeConfig Configuration { get; set; }
    }
}

