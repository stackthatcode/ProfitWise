using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;


namespace ProfitWise.Web.Attributes
{
    public class MaintenanceAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var systemRepository = DependencyResolver.Current.GetService<SystemRepository>();
            var maintenanceActive = systemRepository.RetrieveMaintenanceActive();
            if (!maintenanceActive)
            {
                return;
            }

            var authenticatedContext = filterContext.HttpContext.AuthenticatedContext();
            if (authenticatedContext?.IdentitySnapshot != null && authenticatedContext.IdentitySnapshot.IsAdmin)
            {
                return;
            }


            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                throw new Exception("Attempt to invoke AJAX method while in System Maintenance");
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Content" },
                        { "action", "Maintenance" }
                    });
            }
        }
    }
}
