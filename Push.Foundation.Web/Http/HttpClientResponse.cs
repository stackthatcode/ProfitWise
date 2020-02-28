using System.Collections.Generic;
using System.Net;

namespace Push.Foundation.Web.Http
{
    public class HttpClientResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
}
