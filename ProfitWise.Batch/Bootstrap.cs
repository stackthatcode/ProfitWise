using System;
using System.Diagnostics;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Logging;
using Push.Utilities.General;

namespace ProfitWise.Batch
{
    public class Bootstrap
    {
        public static void ConfigureApp()
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

            LoggerSingleton.Get = NLoggerImpl.RegistrationFactory("ProfitWise.Batch", formatter);
        }
    }
}
