using System.Web;
using System.Web.Mvc;
using Push.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web.Attributes
{
    public class HandleErrorAttributeImpl : HandleErrorAttribute
    {
        public ILogger Logger { get; set; }

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;
            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500) return;
            if (!ExceptionType.IsInstanceOfType(filterContext.Exception)) return;

            // Log the Exception
            Logger.Error(
                    "URL:" + filterContext.HttpContext.Request.Url + " - " +
                    "IsAjaxRequest: " + filterContext.HttpContext.Request.IsAjaxRequest());
            Logger.Error(filterContext.Exception);


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
                model.AspxErrorPath = filterContext.HttpContext.Request.Path;
                model.NavigatedFromAdminArea = true;

                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = new ViewDataDictionary<ErrorModel>(model),
                };
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
