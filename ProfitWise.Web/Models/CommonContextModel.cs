using System.Web.Mvc;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Helpers;
using Push.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class CommonContext
    {
        public string ShopifyApiKey { get; set; }
        public IdentitySnapshot IdentitySnapshot { get; set; }

        // Controlled by View
        public string PageTitle { get; set; }

        public string FullyBrandedPageTitle => 
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);

        public string ShopUrl => "https://" + IdentitySnapshot.ShopDomain;
    }


    public static class CommonContextExtensions
    {
        public static void LoadCommonContextIntoViewBag(this Controller controller)
        {
            controller.ViewBag.CommonContext = new CommonContext
            {
                ShopifyApiKey = ShopifyApiKey.Get(),
                IdentitySnapshot = controller.HttpContext.PullIdentitySnapshot(),
            };
        }
    }
}

