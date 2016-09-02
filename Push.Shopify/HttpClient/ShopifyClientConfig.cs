using Push.Foundation.Web.Http;

namespace Push.Shopify.HttpClient
{
    public class ShopifyClientConfig : HttpClientFacadeConfig
    {
        public ShopifyClientConfig()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 500;

            // These will be more often than not be set at an instance-level
            ThrowExceptionOnBadHttpStatusCode = false;
            RetriesEnabled = true;
        }        
    }
}
