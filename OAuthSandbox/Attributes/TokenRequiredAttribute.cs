using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using ProfitWise.Web.Controllers;
using ProfitWise.Web.Plumbing;
using Push.Shopify.HttpClient;
using Push.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Shopify.Repositories;

namespace ProfitWise.Web.Attributes
{
    public class TokenRequiredAttribute : AuthorizeAttribute
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUserManager _applicationUserManager;
        private readonly ShopifyCredentialService _shopifyCredentialService;
        private readonly ShopifyHttpClient _shopifyHttpClient;
        private readonly ShopifyRequestFactory _requestFactory;


        public TokenRequiredAttribute(
                ApplicationDbContext dbContext,
                ApplicationUserManager applicationUserManager,
                ShopifyCredentialService shopifyCredentialService, 
                ShopifyHttpClient shopifyHttpClient,
                ShopifyRequestFactory requestFactory)
        {
            _dbContext = dbContext;
            _applicationUserManager = applicationUserManager;
            _shopifyCredentialService = shopifyCredentialService;
            _shopifyHttpClient = shopifyHttpClient;
            _requestFactory = requestFactory;
        }

        public string CurrentUserId => HttpContext.Current.ExtractUserId();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Step #1 - Use the Credential Claims Service 
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            
            // Non-existent User - redirect to Authentication
            var user = _applicationUserManager.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (user == null)
            {
                authenticationManager.SignOut();

                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
                return;
            }

            var shopifyCredentialsResults = _shopifyCredentialService.Retrieve(CurrentUserId);
            
            if (!shopifyCredentialsResults.Success)
            {
                DeleteClaimsAndFail(filterContext, shopifyCredentialsResults.Message);
                return;
            }

            // Step #2 - attempt to validate the Access Token 
            var shopifyCredentials = shopifyCredentialsResults.ToShopifyCredentials();
            var request = _requestFactory.HttpGet(shopifyCredentials, "/admin/orders.json?limit=1");
            var response = _shopifyHttpClient.ExecuteRequest(request);

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
            IList<string> _roles = _applicationUserManager.GetRoles(CurrentUserId);
            var isCurrentUserIsAdmin = _roles.Contains(SecurityConfig.AdminRole);


            if (isCurrentUserIsAdmin)
            {
                // Clear the Impersonation Claim
                _shopifyCredentialService.ClearAdminImpersonation(CurrentUserId);

                // Send them over to the Users Screen for Admins
                var route = RouteValueDictionaryExtensions.FromController<AdminHomeController>(x => x.Users(null));
                route["message"] = message;
                filterContext.Result = new RedirectToRouteResult(route);
            }
            else
            {
                // Clear the Impersonation Claim
                _shopifyCredentialService.ClearUserCredentials(CurrentUserId);

                // Send them over to the Shopify Authentication Screen for Users
                var route = RouteValueDictionaryExtensions.FromController<ShopifyAuthController>(x => x.Index(null));
                filterContext.Result = new RedirectToRouteResult(route);
            }
        }        
    }
}

