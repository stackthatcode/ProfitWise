using System;

namespace Push.Foundation.Utilities.Logging
{
    public class LoggerSingleton
    {
        private static IPushLogger _logger = new ConsoleAndDebugLogger();
        
        public static Func<IPushLogger> Get = () => _logger;

        public static void Register(IPushLogger logger)
        {
            _logger = logger;
        }
    }
}
