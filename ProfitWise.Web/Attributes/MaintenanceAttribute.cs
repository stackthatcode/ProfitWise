using System;
using System.Web.Mvc;
using System.Web.Routing;
using ProfitWise.Data.Repositories.System;


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

            // Admins are allowed to use the System while in Maintenance mode
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
                        { "controller", "Error" },
                        { "action", "Maintenance" }
                    });
            }
        }
    }
}
