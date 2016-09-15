using System.Collections.Generic;
using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IEventApiRepository : IShopifyCredentialConsumer
    {
        int RetrieveCount(EventFilter filter);
        IList<Event> Retrieve(EventFilter filter, int page = 1, int limit = 250);
    }
}

