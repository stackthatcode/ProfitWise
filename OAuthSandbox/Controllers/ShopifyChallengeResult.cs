using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace OAuthSandbox.Controllers
{
    internal class ShopifyChallengeResult : HttpUnauthorizedResult
    {
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        private const string ShopNameKey = "ShopName";
        private const string ShopifyOwinProviderName = "Shopify";

        public ShopifyChallengeResult(string redirectUri)
            : this(redirectUri, null, null)
        {
        }
        public ShopifyChallengeResult(string redirectUri, string userId, string shopName)
        {
            RedirectUri = redirectUri;
            UserId = userId;
            ShopName = shopName;
        }

        public string RedirectUri { get; set; }
        public string UserId { get; set; }
        public string ShopName { get; set; }


        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
            {
                properties.Dictionary[XsrfKey] = UserId;
            }
            if (!string.IsNullOrWhiteSpace(this.ShopName))
            {
                properties.Dictionary[ShopNameKey] = this.ShopName;
            }

            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, ShopifyOwinProviderName);
        }
    }
}