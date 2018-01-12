using System;
using System.Diagnostics;
using System.Linq;

namespace Push.Foundation.Utilities.Logging
{
    public static class UtilityExtensions
    {
        static Random random = new Random();

        public static string FullStackTraceDump(this Exception exception)
        {
            var output =
                exception.GetType() + ":" + exception.Message + Environment.NewLine + exception.StackTrace;
            
            if (exception.InnerException != null)
            {
                output = output + 
                    Environment.NewLine + Environment.NewLine +
                    "INNER EXCEPTION" + Environment.NewLine + 
                    exception.InnerException.FullStackTraceDump();
            }
            return output;
        }

        public static string TypeAndMethodName(int stackFrameDepth = 3)
        {
            try
            {
                var stackFrame = new StackFrame(stackFrameDepth);
                var method = stackFrame.GetMethod();
                var type = method.DeclaringType.Name;
                var name = method.Name;
                return type + "." + name;
            }
            catch
            {
                return "";
            }
        }

        public static string RandomHexadecimal(int digits)
        {
            var buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
            {
                return result;
            }
            return result + random.Next(16).ToString("X");
        }
    }
}
