using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IShopApiRepository : IShopifyCredentialConsumer
    {
        Shop Retrieve();
    }
}
