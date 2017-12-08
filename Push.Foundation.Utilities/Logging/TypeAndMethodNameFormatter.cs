using System;
using System.Diagnostics;

namespace Push.Foundation.Utilities.Logging
{
    public class TypeAndMethodWithTraceFormatter : ILogFormatter
    {
        public string Do(string message)
        {
            try
            {
                var stackFrame = new StackFrame(2);
                var method = stackFrame.GetMethod();
                var type = method.DeclaringType.Name;
                var name = method.Name;

                var prefix = type + "." + name + " : ";
                return prefix + message;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
