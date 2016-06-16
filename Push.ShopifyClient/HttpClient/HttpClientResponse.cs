using System.Net;

namespace Push.Shopify.HttpClient
{
    public class HttpClientResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Body { get; set; }
    }
}
