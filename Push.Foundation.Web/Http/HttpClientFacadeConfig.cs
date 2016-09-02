namespace Push.Foundation.Web.Http
{
    public class HttpClientFacadeConfig
    {
        public int RetryLimit { get; set; }
        public int Timeout { get; set; }
        public int ThrottlingDelay { get; set; }
        public bool RetriesEnabled { get; set; }
        public bool ThrowExceptionOnBadHttpStatusCode { get; set; }

        public HttpClientFacadeConfig()
        {
            RetryLimit = 3;
            RetriesEnabled = false;

            Timeout = 60000;
            ThrottlingDelay = 500;
            ThrowExceptionOnBadHttpStatusCode = false;
        }
    }
}
