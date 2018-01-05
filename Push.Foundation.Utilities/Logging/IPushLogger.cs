﻿using System;

namespace Push.Foundation.Utilities.Logging
{
    public interface IPushLogger
    {
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message, string userId = null);
        void Error(Exception exception, string userId = null);
        void Fatal(string message);
        void Fatal(Exception exception);
    }
}
