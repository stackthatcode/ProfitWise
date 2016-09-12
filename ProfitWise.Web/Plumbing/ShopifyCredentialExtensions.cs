using Push.Foundation.Web.Shopify;
using Push.Shopify.HttpClient;

namespace ProfitWise.Web.Plumbing
{
    public static class ShopifyCredentialExtensions
    {
        public static ShopifyCredentials ToShopifyCredentials(this ShopifyCredentialService.RetrieveResult input)
        {
            return new ShopifyCredentials()
            {
                ShopOwnerUserId = input.ShopOwnerUserId,
                ShopDomain = input.ShopDomain,
                AccessToken = input.AccessToken,
            };
        }
    }
}
