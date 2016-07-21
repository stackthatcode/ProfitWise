using System.Security;

namespace ProfitWise.Web.Plumbing
{
    public class ShopifyApiKeyAndSecret
    {
        public SecureString ApiKey { get; set; }
        public SecureString Secret { get; set; }
    }
}
