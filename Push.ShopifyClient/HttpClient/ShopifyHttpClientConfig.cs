namespace Push.Shopify.HttpClient
{
    public class ShopifyHttpClientConfig
    {
        public int ShopifyRetryLimit { get; set; }
        public int ShopifyHttpTimeout { get; set; }
        public int ShopifyThrottlingDelay { get; set; }
        public bool ShopifyRetriesEnabled { get; set; }
        public bool ThrowExceptionOnBadHttpStatusCode { get; set; }

        public ShopifyHttpClientConfig()
        {
            ShopifyRetryLimit = 3;
            ShopifyHttpTimeout = 60000;
            ShopifyThrottlingDelay = 500;

            // These will be more often than not be set at an instance-level
            ThrowExceptionOnBadHttpStatusCode = false;
            ShopifyRetriesEnabled = false;
        }
    }
}
