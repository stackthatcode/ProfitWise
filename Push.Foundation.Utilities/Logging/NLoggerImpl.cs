using System;
using NLog;

namespace Push.Foundation.Utilities.Logging
{
    public class NLoggerImpl : IPushLogger
    {
        private readonly string _loggerName;
        private readonly Func<string, string> _messageFormatter = x => x;

        public static Func<IPushLogger> LoggerFactory(string loggerName, Func<string, string> formatter = null)
        {
            IPushLogger logger = new NLoggerImpl(loggerName, formatter);            
            return () => logger;
        }

        public NLoggerImpl(string loggerName, Func<string, string> formatter)
        {
            _loggerName = loggerName;
            if (formatter != null)
            {
                _messageFormatter = formatter;
            }
        }

        // Get the actual NLog Logger
        public Logger GetLogger => LogManager.GetLogger(_loggerName);

        public bool IsTraceEnabled => GetLogger.IsTraceEnabled;
        public bool IsDebugEnabled => GetLogger.IsDebugEnabled;
        public bool IsInfoEnabled => GetLogger.IsInfoEnabled;
        public bool IsWarnEnabled => GetLogger.IsWarnEnabled;
        public bool IsErrorEnabled => GetLogger.IsErrorEnabled;
        public bool IsFatalEnabled => GetLogger.IsFatalEnabled;


        public void Trace(string message)
        {
            GetLogger.Trace(_messageFormatter(message));
        }

        public void Debug(string message)
        {
            GetLogger.Debug(_messageFormatter(message));
        }

        public void Info(string message)
        {
            GetLogger.Info(_messageFormatter(message));
        }

        public void Warn(string message)
        {
            GetLogger.Warn(_messageFormatter(message));
        }

        public void Error(string message)
        {
            GetLogger.Error(_messageFormatter(message));
        }

        public void Error(Exception exception)
        {
            GetLogger.Error(_messageFormatter(exception.FullStackTraceDump()));
        }

        public void Fatal(string message)
        {
            GetLogger.Fatal(_messageFormatter(message));
        }

        public void Fatal(Exception exception)
        {
            GetLogger.Fatal(_messageFormatter(exception.FullStackTraceDump()));
        }
    }
}
 