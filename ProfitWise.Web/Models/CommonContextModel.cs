using System.Web.Mvc;
using OAuthSandbox.Attributes;
using ProfitWise.Web.Attributes;
using Push.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class CommonContext
    {
        public string ShopifyApiKey { get; set; }
        public UserBrief UserBrief { get; set; }

        // Controlled by View
        public string PageTitle { get; set; }

        public string FullyBrandedPageTitle => 
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);

        public string ShopUrl => "https://" + UserBrief.Domain;
    }


    public static class CommonContextExtensions
    {
        public static void LoadCommonContextIntoViewBag(this Controller controller)
        {
            controller.ViewBag.CommonContext = new CommonContext
            {
                ShopifyApiKey = ShopifyApiKey.Get(),
                UserBrief = controller.HttpContext.RetreiveUserBriefFromContext(),
            };
        }
    }
}

