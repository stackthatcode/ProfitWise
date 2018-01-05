using System;
using NLog;

namespace Push.Foundation.Utilities.Logging
{
    public class NLogger : IPushLogger
    {
        private readonly Logger _nLoggerReference;
        private readonly ILogFormatter _formatter;
        
        public NLogger(string loggerName, ILogFormatter formatter = null)
        {
            _formatter = formatter ?? new DefaultFormatter();
            _nLoggerReference = LogManager.GetLogger(loggerName);
        }
        
        public bool IsTraceEnabled => _nLoggerReference.IsTraceEnabled;
        public bool IsDebugEnabled => _nLoggerReference.IsDebugEnabled;
        public bool IsInfoEnabled => _nLoggerReference.IsInfoEnabled;
        public bool IsWarnEnabled => _nLoggerReference.IsWarnEnabled;
        public bool IsErrorEnabled => _nLoggerReference.IsErrorEnabled;
        public bool IsFatalEnabled => _nLoggerReference.IsFatalEnabled;


        public void Trace(string message)
        {
            _nLoggerReference.Trace(_formatter.Do(message));
        }

        public void Debug(string message)
        {
            _nLoggerReference.Debug(_formatter.Do(message));
        }

        public void Info(string message)
        {
            _nLoggerReference.Info(_formatter.Do(message));
        }

        public void Warn(string message)
        {
            _nLoggerReference.Warn(_formatter.Do(message));
        }

        private string UserIdFormatter(string userId)
        {
            return userId != null ? $"UserId: {userId}|" : "";
        }

        public void Error(string message, string userId = null)
        {
            _nLoggerReference.Error(_formatter.Do(UserIdFormatter(userId) + message));
        }

        public void Error(Exception exception, string userId = null)
        {
            _nLoggerReference.Error(_formatter.Do(UserIdFormatter(userId) + exception.FullStackTraceDump()));
        }

        public void Fatal(string message)
        {
            _nLoggerReference.Fatal(_formatter.Do(message));
        }

        public void Fatal(Exception exception)
        {
            _nLoggerReference.Fatal(_formatter.Do(exception.FullStackTraceDump()));
        }
    }
}
 