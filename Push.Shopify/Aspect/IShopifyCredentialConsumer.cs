using Push.Shopify.HttpClient;

namespace Push.Shopify.Aspect
{
    interface IShopifyCredentialConsumer
    {
        ShopifyCredentials ShopifyCredentials { get; set; }
    }
}
