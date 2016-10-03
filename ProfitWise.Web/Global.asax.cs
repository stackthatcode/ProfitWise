﻿using System;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
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
            // TODO => use this?
            //if (!ConfigurationManager.AppSettings["ErrorHandlingEnabled"].ToBoolTryParse())

            var lastError = Server.GetLastError();
            LoggerSingleton.Get().Error(lastError);

            Server.ClearError();

            var returnUrl = HttpContext.Current.Request.Url.ToString();
            var url = $"/ProfitWise/Error/ServerFault?returnUrl={WebUtility.UrlEncode(returnUrl)}";
            HttpContext.Current.Response.Redirect(url);
        }
    }
}
