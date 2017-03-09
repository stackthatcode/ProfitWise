using Microsoft.AspNet.Identity.Owin;
using Push.Foundation.Web.Identity;
using Push.Shopify.HttpClient;

namespace ProfitWise.Web.Plumbing
{
    public static class CredentialFactory
    {
        public static ShopifyCredentials Make(ExternalLoginInfo externalLoginInfo, ApplicationUser user)
        {
            var domainClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyDomainClaimExternal);
            var accessTokenClaim = externalLoginInfo.ExternalClaim(SecurityConfig.ShopifyOAuthAccessTokenClaimExternal);
            var credentials = new ShopifyCredentials()
            {
                ShopDomain = domainClaim.Value,
                AccessToken = accessTokenClaim.Value,
                ShopOwnerUserId = user.Id,
            };
            return credentials;
        }

    }
}
