using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Helpers
{
    public class RequestActivityIdLogFormatter : ILogFormatter
    {
        // Important Note: Batch and Web follow different logging schemas
        // Web =>  Date|Level|ActivityId|Message
        // Batch => Date|Level|Message (message includes Trace Id)
        //
        public string Do(string message)
        {
            return RequestActivityId.Current + "|" + (message ?? "");
        }
    }
}
