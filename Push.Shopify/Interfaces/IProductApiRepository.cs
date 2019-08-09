using System.Collections.Generic;
using Push.Shopify.Aspect;
using Push.Shopify.Model;

namespace Push.Shopify.Interfaces
{
    public interface IProductApiRepository : IShopifyCredentialConsumer
    {
        int RetrieveCount(ProductFilter filter);
        IList<Product> Retrieve(
            ProductFilter filter, int page = 1, int limit = 250, bool includeCost = false);
    }
}
