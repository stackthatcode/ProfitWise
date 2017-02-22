using System;
using System.Web.Mvc;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class CommonContext
    {
        
        // Controlled by View
        public string PageTitle { get; set; }
        public string FullyBrandedPageTitle =>
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);

        // Populated by IdentityProcessorAttribute
        public string ShopifyApiKey { get; set; }
        public DateTime Today { get; set; }
        public IdentitySnapshot IdentitySnapshot { get; set; }
        public string ShopUrl => "https://" + IdentitySnapshot.ShopDomain;
    }    
}
