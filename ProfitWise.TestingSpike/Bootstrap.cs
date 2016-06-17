using Push.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class Bootstrap
    {
        public static void ConfigureApp()
        {
            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("ProfitWise.Batch");            
        }
    }
}
