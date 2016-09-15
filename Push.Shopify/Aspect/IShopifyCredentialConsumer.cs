using Push.Shopify.HttpClient;

namespace Push.Shopify.Aspect
{
    public interface IShopifyCredentialConsumer
    {
        ShopifyCredentials ShopifyCredentials { get; set; }
    }
}
