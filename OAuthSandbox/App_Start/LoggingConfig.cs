using Push.Utilities.Logging;
using Push.Foundation.Web.Helpers;

namespace ProfitWise.Web
{
    public class LoggingConfig
    {
        public static void Register()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("ProfitWise.Web", ActivityId.MessageFormatter);
        }
    }
}
