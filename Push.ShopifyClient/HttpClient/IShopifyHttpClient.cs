namespace Push.Shopify.HttpClient
{
    public interface IShopifyHttpClient
    {
        HttpClientResponse HttpGet(string path);
    }
}

