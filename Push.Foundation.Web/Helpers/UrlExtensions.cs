using System;
using System.Web;

namespace Push.Foundation.Web.Helpers
{
    public static class UrlExtensions
    {
        public static string ExtractQueryParameter(this string input, string name)
        {
            var index = input.IndexOf("?", StringComparison.Ordinal);
            if (index == -1)
            {
                return null;
            }
            var queryString = input.Substring(index);
            var queryStringCollection = HttpUtility.ParseQueryString(queryString);
            return queryStringCollection[name];
        }
    }
}
