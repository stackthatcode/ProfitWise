using System.Web;
using System.Web.Mvc;
using ProfitWise.Web.Models;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web.Attributes
{
    public class HandleErrorAttributeImpl : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;
            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500) return;
            if (!ExceptionType.IsInstanceOfType(filterContext.Exception)) return;

            // Log the Exception
            try
            {
                var container = DependencyResolver.Current;
                var logger = container.GetService<IPushLogger>();
                logger.Error(
                    "URL:" + filterContext.HttpContext.Request.Url + " - " +
                    "IsAjaxRequest: " + filterContext.HttpContext.Request.IsAjaxRequest());
                logger.Error(filterContext.Exception);
            }
            catch
            {          
                // TODO - create an Event Viewer log entry writer      
            }

            // Notify System Admins
            //ErrorNotification.Send(filterContext.Exception);

            // And now respond
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        activityId = ActivityId.Current,
                        error = true,
                        message = "System Fault - check Logs for Activity Id"
                    }
                };
            }
            else
            {
                var model = new ErrorModel();
                model.AspxErrorPath = filterContext.HttpContext.Request.Url.ToString();
                model.NavigatedFromAdminArea = true;

                var result = new ViewResult();
                result.ViewName = "~/Views/Shared/Error.cshtml";
                result.ViewData = new ViewDataDictionary<ErrorModel>(model);
                CommonContext commonContext = filterContext.Controller.ViewBag.CommonContext;
                result.ViewBag.CommonContext = commonContext;
                filterContext.Result = result;
            }

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }

    public class ErrorModel
    {
        public string AspxErrorPath { get; set; }
        public bool NavigatedFromAdminArea { get; set; }
    }
}
