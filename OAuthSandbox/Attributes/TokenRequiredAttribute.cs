using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using OAuthSandbox.Controllers;
using ProfitWise.Web.Controllers;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Utilities.Web.Helpers;
using Push.Utilities.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class TokenRequiredAttribute : AuthorizeAttribute
    {
        public string CurrentUserId
        {
            get
            {
                return HttpContext.Current.ExtractUserId();
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Step #1 - Use the Credential Claims Service 
            var dbContext = ApplicationDbContext.Create();
            var owinContext = HttpContext.Current.GetOwinContext();
            var userManager = owinContext.GetUserManager<ApplicationUserManager>();

            // Non-existent User - redirect to Authentication
            var user = userManager.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (user == null)
            {
                var authenticationManager = owinContext.Authentication;
                authenticationManager.SignOut();

                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
                return;
            }


            var credentialsService = new ShopifyCredentialService(owinContext.GetUserManager<ApplicationUserManager>());
            var shopifyCredentials = credentialsService.Retrieve(CurrentUserId);
            
            if (!shopifyCredentials.Success)
            {
                DeleteClaimsAndFail(filterContext, dbContext, shopifyCredentials.Message);
                return;
            }

            // Step #2 - attempt to validate the Access Token 
            var httpClient = new HttpClient();
            var logger = LoggerSingleton.Get();
            var client = 
                new ShopifyHttpClient(
                        httpClient, logger, shopifyCredentials.ShopDomain, shopifyCredentials.AccessToken);

            var response = client.HttpGet("/admin/orders.json?limit=1");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                DeleteClaimsAndFail(filterContext, dbContext, "Invalid Shopify Access Token Claim stored for impersonated User; they will need to refresh their Shopify credentials");
            }
        }


        private void DeleteClaimsAndFail(AuthorizationContext filterContext, ApplicationDbContext dbContext, string message = "")
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            var owinContext = HttpContext.Current.GetOwinContext();
            var credentialsService = new ShopifyCredentialService(owinContext.GetUserManager<ApplicationUserManager>());

            IList<string> _roles = userManager.GetRoles(CurrentUserId);
            var isCurrentUserIsAdmin = _roles.Contains(SecurityConfig.AdminRole);


            if (isCurrentUserIsAdmin)
            {
                // Clear the Impersonation Claim
                credentialsService.ClearAdminImpersonation(CurrentUserId);

                // Send them over to the Users Screen for Admins
                var route = RouteValueDictionaryExtensions.FromController<AdminHomeController>(x => x.Users(null));
                route["message"] = message;
                filterContext.Result = new RedirectToRouteResult(route);
            }
            else
            {
                // Clear the Impersonation Claim
                credentialsService.ClearUserCredentials(CurrentUserId);

                // Send them over to the Shopify Authentication Screen for Users
                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
            }
        }
        
    }
}

