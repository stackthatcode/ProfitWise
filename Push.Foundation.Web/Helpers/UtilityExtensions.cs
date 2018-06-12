using System.Collections.Generic;
using System.Web;

namespace Push.Foundation.Web.Helpers
{
    public static class UtilityExtensions
    {
        public static Dictionary<string, string> QueryStringToDictionary(this HttpContextBase context)
        {
            var output = new Dictionary<string, string>();
            foreach (string key in context.Request.QueryString.Keys)
            {
                output[key] = context.Request.QueryString[key];
            }
            return output;
        }
    }
}
