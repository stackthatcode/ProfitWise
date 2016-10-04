using System;
using System.Net;

namespace Push.Foundation.Web.Http
{
    public class BadHttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public BadHttpStatusCodeException(HttpStatusCode statusCode, string message) 
                    : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
