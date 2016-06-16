namespace Push.Shopify.HttpClient
{
    public interface IShopifyHttpClient
    {
        HttpClientResponse HttpGet(string path);
        int ShopifyRetryLimit { get; set; }
        int ShopifyHttpTimeout { get; set; }
        int ShopifyThrottlingDelay { get; set; }
        bool ShopifyRetriesEnabled { get; set; }
        bool ThrowExceptionOnBadHttpStatusCode { get; set; }
    }
}
