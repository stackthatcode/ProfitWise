namespace ProfitWise.Web.Models
{
    public class JavaScriptRedirectModel
    {
        public string Header { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }


        public static JavaScriptRedirectModel BuildForChargeConfirm(string url)
        {
            return new JavaScriptRedirectModel
            {
                Url = url,
                Header = "ProfitWise - Confirm Subscription Sign-up",
                SubTitle = "Redirecting to Shopify...",
            };
        }
        
        public static JavaScriptRedirectModel BuildForOAuth(string url)
        {
            return new JavaScriptRedirectModel
            {
                Url = url,
                Header = "ProfitWise - Authorizing through Shopify",
                SubTitle = "Redirecting to Shopify...",
            };
        }
    }
}
