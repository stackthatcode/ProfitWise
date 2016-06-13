using System;
using Push.Utilities.Logging;

namespace Push.Utilities.Logging
{
    public class LoggerSingleton
    {
        public static Func<ILogger> Get = () => new ConsoleAndDebugLogger();
    }
}
