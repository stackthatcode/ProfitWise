using System;
using NLog;

namespace Push.Foundation.Utilities.Logging
{
    public class NLogger : IPushLogger
    {
        private readonly Logger _nLoggerReference;
        private readonly Func<string, string> _messageFormatter = x => x;

        public NLogger(string loggerName, Func<string, string> formatter = null)
        {
            _nLoggerReference = LogManager.GetLogger(loggerName);

            if (formatter != null)
            {
                _messageFormatter = formatter;
            }
        }
        
        public bool IsTraceEnabled => _nLoggerReference.IsTraceEnabled;
        public bool IsDebugEnabled => _nLoggerReference.IsDebugEnabled;
        public bool IsInfoEnabled => _nLoggerReference.IsInfoEnabled;
        public bool IsWarnEnabled => _nLoggerReference.IsWarnEnabled;
        public bool IsErrorEnabled => _nLoggerReference.IsErrorEnabled;
        public bool IsFatalEnabled => _nLoggerReference.IsFatalEnabled;


        public void Trace(string message)
        {
            _nLoggerReference.Trace(_messageFormatter(message));
        }

        public void Debug(string message)
        {
            _nLoggerReference.Debug(_messageFormatter(message));
        }

        public void Info(string message)
        {
            _nLoggerReference.Info(_messageFormatter(message));
        }

        public void Warn(string message)
        {
            _nLoggerReference.Warn(_messageFormatter(message));
        }

        public void Error(string message)
        {
            _nLoggerReference.Error(_messageFormatter(message));
        }

        public void Error(Exception exception)
        {
            _nLoggerReference.Error(_messageFormatter(exception.FullStackTraceDump()));
        }

        public void Fatal(string message)
        {
            _nLoggerReference.Fatal(_messageFormatter(message));
        }

        public void Fatal(Exception exception)
        {
            _nLoggerReference.Fatal(_messageFormatter(exception.FullStackTraceDump()));
        }
    }
}
 