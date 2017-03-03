using Push.Foundation.Web.Interfaces;
using Push.Shopify.HttpClient;

namespace ProfitWise.Data.Utility
{
    public static class CredentialResultExtensions
    {
        public static ShopifyCredentials ToShopifyCredentials(this CredentialServiceResult input)
        {
            return new ShopifyCredentials
            {
                ShopOwnerUserId = input.ShopOwnerUserId,
                ShopDomain = input.ShopDomain,
                AccessToken = input.AccessToken,
            };
        }
    }
}
