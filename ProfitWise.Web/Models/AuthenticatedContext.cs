using System;
using ProfitWise.Web.Attributes;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class AuthenticatedContext
    {
        // Populated by IdentityProcessorAttribute
        public string ShopifyApiKey { get; set; }
        public DateTime Today { get; set; }
        public IdentitySnapshot IdentitySnapshot { get; set; }
        public string ShopUrl => "https://" + IdentitySnapshot.ShopDomain;
        public bool IsImpersonated => IdentitySnapshot != null && IdentitySnapshot.Impersonated;
        

        // Controlled by View
        public string PageTitle { get; set; }
        public string FullyBrandedPageTitle =>
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);
    }
}
