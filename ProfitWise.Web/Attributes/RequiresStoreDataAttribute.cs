using System.Web.Mvc;
using ProfitWise.Web.Plumbing;

namespace ProfitWise.Web.Attributes
{
    public class RequiresStoreDataAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {            
            // Pull the User ID from OWIN plumbing...
            var identity = filterContext.HttpContext.IdentitySnapshot();
            var currentUrl = filterContext.HttpContext.Request.Url.PathAndQuery;

            if (!identity.PwShop.IsDataLoaded)
            {
                filterContext.Result = GlobalConfig.Redirect("/Content/Welcome", currentUrl);
            }            
        }
    }
}
