using Push.Foundation.Web.Identity;
using Push.Shopify.HttpClient;

namespace ProfitWise.Web.Plumbing
{
    public static class ShopifyCredentialExtensions
    {
        public static ShopifyCredentials ToShopifyCredentials(this ShopifyCredentialService.RetrieveResult input)
        {
            return new ShopifyCredentials()
            {
                ShopOwnerId = input.ShopOwnerUserId,
                ShopDomain = input.ShopDomain,
                AccessToken = input.AccessToken,
            };
        }
    }
}
