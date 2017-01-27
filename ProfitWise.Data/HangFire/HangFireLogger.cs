using System;
using Hangfire.Logging;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.HangFire
{
    // Glue between the Push.Foundation Logger and HangFire
    // NOTE: Requires registration of LoggerSingleton delegate
    public class HangFireLogger : ILog
    {
        private static readonly IPushLogger Logger = LoggerSingleton.Get();

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            if (Logger == null)
            {
                return false;
            }
            if (messageFunc == null && exception == null)
            {
                return true;
            }


            if (logLevel == LogLevel.Trace && Logger.IsTraceEnabled)
            {
                Logger.Trace(messageFunc());
            }
            if (logLevel == LogLevel.Debug && Logger.IsDebugEnabled)
            {
                Logger.Debug(messageFunc());
            }
            if (logLevel == LogLevel.Info && Logger.IsInfoEnabled)
            {
                Logger.Info(messageFunc());
            }
            if (logLevel == LogLevel.Warn && Logger.IsWarnEnabled)
            {
                Logger.Warn(messageFunc());
            }
            if (logLevel == LogLevel.Error && Logger.IsErrorEnabled)
            {
                var message = messageFunc();
                if (!message.IsNullOrEmpty())
                {
                    Logger.Error(message);
                }
                Logger.Error(exception);
            }
            if (logLevel == LogLevel.Fatal && Logger.IsFatalEnabled)
            {
                Logger.Fatal(messageFunc());

            }
            return true;
        }
    }
}
