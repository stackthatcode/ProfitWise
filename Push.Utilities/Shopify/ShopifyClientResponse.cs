using System.Net;

namespace Push.Utilities.Shopify
{
    public class ShopifyClientResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ResponseBody { get; set; }
    }
}