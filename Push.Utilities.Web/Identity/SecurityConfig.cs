namespace Push.Utilities.Web.Identity
{
    public class SecurityConfig
    {

        // External synonymous with "does not live in database"
        public const string ShopifyOAuthAccessTokenClaimExternal = "urn:shopify:oauth:accesstoken:external";
        public const string ShopifyDomainClaimExternal = "urn:shopify:shopname:external";


        public const string ShopifyOAuthAccessTokenClaim = "urn:shopify:oauth:accesstoken";
        public const string ShopifyDomainClaim = "urn:shopify:shopname";
        public const string UserImpersonationClaim = "urn:profitwise:impersonation:userid";

        public const string DefaultAdminEmail = "info@3duniverse.org";
        public const string DefaultAdminPassword = "123456";

        public const string UserRole = "USER";
        public const string AdminRole = "ADMIN";
    }
}
