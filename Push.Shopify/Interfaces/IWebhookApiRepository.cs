using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IWebhookApiRepository : IShopifyCredentialConsumer
    {
        Webhook Subscribe(Webhook request);
        Webhook Retrieve(long id);
        Webhook UpdateAddress(Webhook request);
    }
}
