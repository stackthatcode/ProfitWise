using System;
using Push.Utilities.Logging;

namespace Push.Utilities.Logging
{
    public class LoggerSingleton
    {
        public static Func<IPushLogger> Get = () => new ConsoleAndDebugLogger();
    }
}
