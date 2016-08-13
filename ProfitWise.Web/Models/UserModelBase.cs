﻿using System.Web.Mvc;
using OAuthSandbox.Attributes;
using ProfitWise.Web.Attributes;
using Push.Utilities.Helpers;

namespace ProfitWise.Web.Models
{
    public class UserModelBase
    {
        public string ShopifyApiKey { get; set; }
        public UserBrief UserBrief { get; set; }

        // Controlled by View
        public string PageTitle { get; set; }

        public string FullyBrandedPageTitle => 
            "ProfitWise - " + (PageTitle.IsNullOrEmpty() ? "Know Your Profitability" : PageTitle);
    }

    public static class UserModelExtensions
    {
        // TODO - use Generics for subclasses of the type
        public static UserModelBase UserModelFactory(this Controller controller)
        {
            return new UserModelBase()
            {
                ShopifyApiKey = ShopifyApiKey.Get(),
                UserBrief = controller.HttpContext.RetreiveUserBriefFromContext(),
            };
        }
    }
}

