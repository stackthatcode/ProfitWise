using System;

namespace Push.Foundation.Utilities.Logging
{
    public class ConsoleAndDebugLogger : IPushLogger
    {
        public bool IsTraceEnabled => true;
        public bool IsDebugEnabled => true;
        public bool IsInfoEnabled => true;
        public bool IsWarnEnabled => true;
        public bool IsErrorEnabled => true;
        public bool IsFatalEnabled => true;

        public void Trace(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Debug(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Warn(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Error(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Error(Exception exception)
        {
            Console.WriteLine(exception.StackTrace);
            System.Diagnostics.Debug.WriteLine(exception.StackTrace);
        }

        public void Fatal(string message)
        {
            Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Fatal(Exception exception)
        {
            Console.WriteLine(exception.StackTrace);
            System.Diagnostics.Debug.WriteLine(exception.StackTrace);
        }

    }
}
