﻿using System;
using System.Diagnostics;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Batch
{
    public class Bootstrap
    {
        public static void ConfigureApp()
        {
            var container = AutofacRegistration.Build();
            ConfigureHangFire();
            ConfigureLogging();
        }

        private static void ConfigureHangFire()
        {
        }


        private static void ConfigureLogging()
        {
            Func<string, string> formatter = x =>
            {
                var stackFrame = new StackFrame(2);
                var method = stackFrame.GetMethod();
                var type = method.DeclaringType.Name;
                var name = method.Name;

                var prefix = type + "." + name + " : ";
                return prefix + x;
            };

            LoggerSingleton.Get = NLoggerImpl.LoggerFactory("ProfitWise.Batch", formatter);
        }
    }
}
