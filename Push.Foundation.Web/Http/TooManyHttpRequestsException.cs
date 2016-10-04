using System;
using System.Net;

namespace Push.Foundation.Web.Http
{
    public class TooManyHttpRequestsException : Exception
    {
        public HttpStatusCode StatusCode = (HttpStatusCode) 429;

        public TooManyHttpRequestsException(string message) 
                    : base(message)
        {
        }
    }
}
