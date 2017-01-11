using System.Web;
using System.Web.Mvc;
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
                // TODO - unable to create an Error Log - what to do, now???
            }
            
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // This will please a JSON message in the response
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
                // This will render the ServerFault page...
                var model = new ErrorModel()
                {
                    ReturnUrl = filterContext.HttpContext.Request.Url?.ToString()
                };
                var result = new ViewResult();
                result.ViewName = "~/Views/Error/Http500.cshtml";
                result.ViewData = new ViewDataDictionary<ErrorModel>(model);                
                filterContext.Result = result;
            }

            // ... and this ensures proper HTTP Status Codes are returned
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }

    public class ErrorModel
    {
        public string ReturnUrl { get; set; }
        public bool NavigatedFromAdminArea { get; set; }
    }
}
