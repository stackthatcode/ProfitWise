using System;

namespace Push.Foundation.Utilities.Logging
{
    public interface IPushLogger
    {
        bool IsTraceEnabled { get; }
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(Exception exception);
        void Fatal(string message);
    }
}
