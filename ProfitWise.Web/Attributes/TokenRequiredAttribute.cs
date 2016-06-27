using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProfitWise.Web.Controllers;
using ProfitWise.Web.Plumbing;
using Push.Shopify.HttpClient;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;

namespace ProfitWise.Web.Attributes
{
    public class TokenRequiredAttribute : AuthorizeAttribute
    {
        public string CurrentUserId => HttpContext.Current.ExtractUserId();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Service Locator anti-pattern => only using as this is an attribute }:-)
            var container = DependencyResolver.Current;
            var applicationUserManager = container.GetService<ApplicationUserManager>();
            var shopifyCredentialService = container.GetService<ShopifyCredentialService>();
            var shopifyHttpClient = container.GetService<ShopifyHttpClient>();
            var requestFactory = container.GetService<ShopifyRequestFactory>();


            // Step #1 - Use the Credential Claims Service 
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            
            // Non-existent User - redirect to Authentication
            var user = applicationUserManager.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (user == null)
            {
                authenticationManager.SignOut();

                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
                return;
            }

            var shopifyCredentialsResults = shopifyCredentialService.Retrieve(CurrentUserId);
            
            if (!shopifyCredentialsResults.Success)
            {
                DeleteClaimsAndFail(filterContext, shopifyCredentialsResults.Message);
                return;
            }

            // Step #2 - attempt to validate the Access Token 
            var shopifyCredentials = shopifyCredentialsResults.ToShopifyCredentials();
            var request = requestFactory.HttpGet(shopifyCredentials, "/admin/orders.json?limit=1");
            var response = shopifyHttpClient.ExecuteRequest(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                DeleteClaimsAndFail(filterContext, "Invalid Shopify Access Token Claim stored for impersonated User; they will need to refresh their Shopify credentials");
            }
        }


        private void DeleteClaimsAndFail(AuthorizationContext filterContext, string message = "")
        {
            // Service Locator anti-pattern => only using as this is an attribute }:-)
            var container = DependencyResolver.Current;
            var applicationUserManager = container.GetService<ApplicationUserManager>();
            var shopifyCredentialService = container.GetService<ShopifyCredentialService>();
            var roles = applicationUserManager.GetRoles(CurrentUserId);
            var isCurrentUserIsAdmin = roles.Contains(SecurityConfig.AdminRole);


            if (isCurrentUserIsAdmin)
            {
                // Clear the Impersonation Claim
                shopifyCredentialService.ClearAdminImpersonation(CurrentUserId);

                // Send them over to the Users Screen for Admins
                var route = RouteValueDictionaryExtensions.FromController<AdminHomeController>(x => x.Users(null));
                route["message"] = message;
                filterContext.Result = new RedirectToRouteResult(route);
            }
            else
            {
                // Clear the Impersonation Claim
                shopifyCredentialService.ClearUserCredentials(CurrentUserId);

                // Send them over to the Shopify Authentication Screen for Users
                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
            }
        }        
    }
}

