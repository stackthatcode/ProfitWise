namespace Push.Foundation.Web.Identity
{
    public class SecurityConfig
    {
        // External Claim keys - external is synonymous with "does not live in database"
        public const string ShopifyOAuthAccessTokenClaimExternal = "urn:shopify:oauth:accesstoken:external";
        public const string ShopifyDomainClaimExternal = "urn:shopify:shopname:external";

        // Internal Claim keys - internal means they "live in our database"
        public const string ShopifyOAuthAccessTokenClaim = "urn:shopify:oauth:accesstoken";
        public const string ShopifyDomainClaim = "urn:shopify:shopname";
        public const string UserImpersonationClaim = "urn:profitwise:impersonation:userid";

        // Security Defaults
        public const string DefaultAdminEmail = "info@3duniverse.org";
        public const string DefaultAdminPassword = "123456";

        // Profitwise Role keys
        public const string UserRole = "USER";
        public const string AdminRole = "ADMIN";
    }
}
