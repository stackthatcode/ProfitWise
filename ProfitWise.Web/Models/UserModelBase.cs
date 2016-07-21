using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OAuthSandbox.Attributes;
using ProfitWise.Web.Attributes;

namespace ProfitWise.Web.Models
{
    public class UserModelBase
    {
        public UserBrief UserBrief { get; set; }
        public string PageTitle { get; set; }
        public string ShopifyApiKey { get; set; }
    }

    public static class UserModelExtensions
    {
        public static UserModelBase UserModelFactory(this Controller controller)
        {
            return new UserModelBase()
            {
                ShopifyApiKey = ShopifyApiKey.Get(),
                UserBrief = controller.HttpContext.RetreiveUserBriefFromContext(),
                PageTitle = controller.ControllerContext.FriendlyActionName(),
            };
        }
    }
}