using System;
using System.Net;
using System.Web;


namespace Push.Foundation.Web.Helpers
{
    public static class HttpExceptionUtilities
    {
        public static bool IsHttpExceptionWithCode(this Exception exception, int statusCode)
        {
            var httpException = exception as HttpException;
            return httpException != null && httpException.GetHttpCode() == statusCode;
        }

        public static bool IsHttpExceptionWithCode(this Exception exception, HttpStatusCode code)
        {
            return exception.IsHttpExceptionWithCode((int) code);
        }

        public static bool IsHttpException(this Exception exception)
        {
            return exception is HttpException;
        }
    }
}

