using System.Collections.Generic;
using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IRecurringApiRepository : IShopifyCredentialConsumer
    {
        RecurringApplicationCharge UpsertCharge(RecurringApplicationCharge input);
        RecurringApplicationCharge RetrieveCharge(string id);
    }
}
