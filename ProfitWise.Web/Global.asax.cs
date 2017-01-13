using System;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ProfitWise.Web.Controllers;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
        }


        protected void Application_Error(object sender, EventArgs e)
        {
            // Log the error
            var lastError = Server.GetLastError();
            LoggerSingleton.Get().Error(lastError);

            // Instantiate the Error Controller
            IController errorController = DependencyResolver.Current.GetService<ErrorController>();

            // Build the route
            var errorRoute = new RouteData();
            errorRoute.Values.Add("controller", "Error");
            var httpException = lastError as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                errorRoute.Values.Add("action", "Http404");
            }
            else
            {
                errorRoute.Values.Add("action", "Http500");
            }
            errorRoute.Values.Add("returnUrl", HttpContext.Current.Request.Url.OriginalString);
            
            // Clear the error on Server and the Reponse
            Server.ClearError();
            Response.Clear();

            var context = new HttpContextWrapper(HttpContext.Current);
            errorController.Execute(new RequestContext(context, errorRoute));            
        }
    }
}
