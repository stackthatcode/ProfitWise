namespace Push.Foundation.Utilities.Logging
{
    public class DefaultFormatter : ILogFormatter
    {
        public string Do(string message)
        {
            return message;
        }
    }
}
