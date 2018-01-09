using System.Web;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web.Plumbing
{
    public class LogFormatter : ILogFormatter
    {
        // Important Note: Batch and Web follow different logging schemas
        // Web =>  Date|Level|ActivityId + ShopId|Message
        // Batch => Date|Level|TraceId|Message
        public string Do(string message)
        {
            var activityId = RequestActivityId.Current;
            var shopId = HttpContext.Current.CurrentPwShopId().IsNullOrEmptyAlt("(No Shop Id)");
            return $"{activityId} - {shopId}|{message ?? ""}";
        }
    }
}
