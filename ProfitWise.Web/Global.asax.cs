using System;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.General;

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
            //if (!ConfigurationManager.AppSettings["ErrorHandlingEnabled"].ToBoolTryParse())
            //{
            //    return;
            //}

            var lastError = Server.GetLastError();
            LoggerSingleton.Get().Error(lastError);
            //ErrorNotification.Send(lastError);

            Server.ClearError();

            //var redirectUrl = ConfigurationManager.AppSettings["AdminErrorRedirect"];
            //if (redirectUrl != null)
            //{
            //    HttpContext.Current.Response.Redirect(redirectUrl);
            //}
        }
    }
}
