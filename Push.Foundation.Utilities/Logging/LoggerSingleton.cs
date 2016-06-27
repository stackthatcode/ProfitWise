using System;

namespace Push.Foundation.Utilities.Logging
{
    public class LoggerSingleton
    {
        public static Func<IPushLogger> Get = () => new ConsoleAndDebugLogger();
    }
}
