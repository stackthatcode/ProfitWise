using System;
using System.Net;

namespace Push.Shopify.HttpClient
{
    public class BadShopifyHttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public BadShopifyHttpStatusCodeException(HttpStatusCode statusCode) 
                    : base("Shopify returned bad Http Status Code: " + statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
