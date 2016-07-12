using System.Diagnostics;

namespace Push.Foundation.Utilities.General
{
    public static class CurrentMethodName
    {
        public static string ClassAndMethodName(this object input)
        {
            return input.GetType().Name + "." + new StackFrame(1).GetMethod().Name;
        }
    }
}
