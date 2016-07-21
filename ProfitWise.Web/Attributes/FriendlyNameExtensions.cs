using System.Linq;
using System.Web.Mvc;

namespace ProfitWise.Web.Attributes
{
    public static class FriendlyNameExtensions
    {
        public static string FriendlyActionName(this ControllerContext context)
        {
            var actionName = context.RouteData.Values["action"].ToString();
            var methodInfo = context.Controller.GetType().GetMethod(actionName);

            var attribute =
                methodInfo.GetCustomAttributes(typeof(FriendlyNameAttribute), false).FirstOrDefault() as
                    FriendlyNameAttribute;
            if (attribute != null)
            {
                return attribute.FriendlyName;
            }
            else
            {
                return "";
            }
        }
    }
}