using System.Web.Mvc;
using OAuthSandbox.Attributes;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttributeImpl());

            filters.Add(new IdentityCachingAttribute());
        }
    }
}
