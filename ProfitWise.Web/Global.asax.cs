using System;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Controllers;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Everything here is starting after Startup class has executed...
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
        }


        protected void Application_Error(object sender, EventArgs e)
        {
            // Explicitly instantiate dependencies
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            IController errorController = DependencyResolver.Current.GetService<ErrorController>();

            var lastError = Server.GetLastError();
            
            // We log everything except for HTTP 404's
            if (!lastError.IsHttpExceptionWithCode(404))
            {
                logger.Error(lastError);
            }
            
            // Build the route based on error type
            var errorRoute = new RouteData();
            errorRoute.Values.Add("controller", "Error");

            if (lastError.IsHttpExceptionWithCode(404))
            {
                errorRoute.Values.Add("action", "Http404");
            }
            else if (lastError.IsHttpExceptionWithCode(403))
            {
                errorRoute.Values.Add("action", "Http403");
            }
            else
            {
                errorRoute.Values.Add("action", "Http500");
            }

            // Clear the error on Server and the Reponse
            Server.ClearError();
            Response.Clear();

            var context = new HttpContextWrapper(HttpContext.Current);
            errorController.Execute(new RequestContext(context, errorRoute));            
        }
    }
}
