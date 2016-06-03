using System.Web;
using System.Web.Mvc;
using OAuthSandbox.Attributes;

namespace OAuthSandbox
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            filters.Add(new IdentityCachingAttribute());
        }
    }
}
