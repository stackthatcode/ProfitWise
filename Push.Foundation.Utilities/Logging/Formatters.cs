using System;
using System.Diagnostics;

namespace Push.Foundation.Utilities.Logging
{
    public class Formatters
    {
        public static Func<string, string> TypeAndMethodNameFormatFactory()
        {
          return x =>
                {
                    var stackFrame = new StackFrame(2);
                    var method = stackFrame.GetMethod();
                    var type = method.DeclaringType.Name;
                    var name = method.Name;

                    var prefix = type + "." + name + " : ";
                    return prefix + x;
                };
        }
    }
}

