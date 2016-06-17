using System.Net;

namespace Push.Shopify.HttpClient
{
    public interface IShopifyHttpClient
    {
        HttpClientResponse ExecuteRequest(HttpWebRequest request);
    }
}
