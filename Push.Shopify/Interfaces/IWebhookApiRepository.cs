using System.Collections.Generic;
using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IWebhookApiRepository : IShopifyCredentialConsumer
    {
        Webhook Subscribe(Webhook request);
        Webhook Retrieve(string topic, string address);
        Webhook Retrieve(string topic);
        List<Webhook> RetrieveAll();
        Webhook UpdateAddress(Webhook request);
        void Delete(long webHookId);
    }
}
