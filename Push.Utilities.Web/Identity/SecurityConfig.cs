namespace Push.Utilities.Web.Identity
{
    public class SecurityConfig
    {
        public const string ShopifyOAuthAccessTokenClaim = "urn:shopify:oauth:accesstoken";
        public const string ShopifyDomainClaim = "urn:shopify:shopname";
        public const string UserImpersonationClaim = "urn:profitwise:imperation:userid";

        public const string DefaultAdminEmail = "info@3duniverse.org";
        public const string DefaultAdminPassword = "123456";

        public const string UserRole = "USER";
        public const string AdminRole = "ADMIN";
    }
}
