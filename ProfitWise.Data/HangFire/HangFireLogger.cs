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
        private readonly IPushLogger _logger;

        public HangFireLogger(IPushLogger logger)
        {
            _logger = logger;
        }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            if (_logger == null)
            {
                return false;
            }
            if (messageFunc == null && exception == null)
            {
                return true;
            }


            if (logLevel == LogLevel.Trace && _logger.IsTraceEnabled)
            {
                _logger.Trace(messageFunc());
            }
            if (logLevel == LogLevel.Debug && _logger.IsDebugEnabled)
            {
                _logger.Debug(messageFunc());
            }
            if (logLevel == LogLevel.Info && _logger.IsInfoEnabled)
            {
                _logger.Info(messageFunc());
            }
            if (logLevel == LogLevel.Warn && _logger.IsWarnEnabled)
            {
                _logger.Warn(messageFunc());
            }
            if (logLevel == LogLevel.Error && _logger.IsErrorEnabled)
            {
                var message = messageFunc();
                if (!message.IsNullOrEmpty())
                {
                    _logger.Error(message);
                }
                _logger.Error(exception);
            }
            if (logLevel == LogLevel.Fatal && _logger.IsFatalEnabled)
            {
                _logger.Fatal(messageFunc());

            }
            return true;
        }
    }
}
