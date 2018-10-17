namespace ProfitWise.Web.Models
{
    public class OAuthRedirectModel
    {
        public string OAuthAdminPath { get; set; }
        public string ShopDomain { get; set; }

        public OAuthRedirectModel(string oAuthAdminPath, string shopDomain)
        {
            OAuthAdminPath = oAuthAdminPath;
            ShopDomain = shopDomain;
        }
        
    }
}
