using Push.Shopify.HttpClient;

namespace Push.Shopify.Repositories
{
    interface IShopifyCredentialConsumer
    {
        ShopifyCredentials ShopifyCredentials { get; set; }
    }
}
