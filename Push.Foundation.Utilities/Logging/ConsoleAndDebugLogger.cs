﻿using System;

namespace Push.Foundation.Utilities.Logging
{
    public class ConsoleAndDebugLogger : IPushLogger
    {
        public bool IsTraceEnabled => true;

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
    }
}