using System;
using System.Web;

namespace Push.Foundation.Web.Helpers
{
    public class RequestActivityId
    {
        public const string ActivityIdKey = "ActivityIdKey";

        public static Guid Current
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return Guid.Empty;
                }
                if (HttpContext.Current.Items[ActivityIdKey] == null)
                {
                    HttpContext.Current.Items[ActivityIdKey] = Guid.NewGuid();
                }
                return Guid.Parse(HttpContext.Current.Items[ActivityIdKey].ToString());
            }
        }        
    }
}
