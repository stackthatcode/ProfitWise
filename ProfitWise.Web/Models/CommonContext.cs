using System.Web.Mvc;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class CommonContext
    {
        // Shopify API Key
        public string ShopifyApiKey { get; set; }

        // Controlled by View
        public string PageTitle { get; set; }

        public string FullyBrandedPageTitle =>
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);

        // Identity Snapshot
        public IdentitySnapshot IdentitySnapshot { get; set; }
        public string ShopUrl => "https://" + IdentitySnapshot.ShopDomain;
    }    
}
