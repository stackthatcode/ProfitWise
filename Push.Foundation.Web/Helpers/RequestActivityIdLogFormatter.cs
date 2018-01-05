using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.Helpers
{
    public class RequestActivityIdLogFormatter : ILogFormatter
    {
        public string Do(string message)
        {
            return RequestActivityId.Current + "|" + (message ?? "");
        }
    }
}
