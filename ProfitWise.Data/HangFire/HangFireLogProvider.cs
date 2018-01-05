using Hangfire.Logging;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.HangFire
{
    public class HangFireLogProvider : ILogProvider
    {
        private static IPushLogger _loggerInstance;

        public static void RegisterInstance(IPushLogger loggerInstance)
        {
            _loggerInstance = loggerInstance;
        }

        public ILog GetLogger(string name)
        {
            return new HangFireLogger(_loggerInstance);
        }
    }
}
