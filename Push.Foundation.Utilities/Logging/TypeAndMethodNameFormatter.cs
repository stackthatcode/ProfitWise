namespace Push.Foundation.Utilities.Logging
{
    public class TypeAndMethodNameFormatter : ILogFormatter
    {
        public string Do(string message)
        {
            return UtilityExtensions.TypeAndMethodName() + " : " + message;
        }
    }
}
