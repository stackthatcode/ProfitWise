using System.Collections.Generic;
using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IOrderApiRepository : IShopifyCredentialConsumer
    {
        int RetrieveCount(OrderFilter filter);
        IList<Order> Retrieve(OrderFilter filter, int page = 1, int limit = 250);
    }
}
