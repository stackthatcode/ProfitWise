using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Configuration;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using ProfitWise.Web.Models;
using ProfitWise.Web.Plumbing;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;


namespace ProfitWise.Web.Attributes
{
    public class IdentityProcessorAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dbContext = DependencyResolver.Current.GetService<ApplicationDbContext>();
            var roleManager = DependencyResolver.Current.GetService<ApplicationRoleManager>();
            var factory = DependencyResolver.Current.GetService<MultitenantFactory>();
            var credentialService = DependencyResolver.Current.GetService<IShopifyCredentialService>();
            var shopRepository = DependencyResolver.Current.GetService<ShopRepository>();
            var signInManager = DependencyResolver.Current.GetService<ApplicationSignInManager>();
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            var timeZoneTranslator = DependencyResolver.Current.GetService<TimeZoneTranslator>();

            // Pull the User ID from OWIN plumbing...
            var currentUrl = filterContext.HttpContext.Request.Url.ToString();
            var shopParameter = filterContext.HttpContext.Request.QueryString["shop"];
            var userId = filterContext.HttpContext.User.ExtractUserId();            


            // Get the Shop Credentials
            var result = credentialService.Retrieve(userId);
            if (result.Success == false)
            {
                // On failure of credential service, throw an error, which will redirect to Server Fault page
                logger.Error(
                    $"Unable to Retrieve Claims for User {userId}: '{result.Message}' - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, currentUrl);
                return;
            }

            // We may have an impersonated User
            var effectiveUserId = result.ShopOwnerUserId;
            var pwShop = shopRepository.RetrieveByUserId(effectiveUserId);

            if (shopParameter != null && shopParameter != pwShop.Domain)
            {
                // Oops - wrong domain!
                logger.Error($"Currently logged in as {pwShop.Domain}, while accessing App for {shopParameter}");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.ExternalLoginFailureUrl, currentUrl);
                return;
            }

            if (!pwShop.IsShopEnabled)
            {
                logger.Info($"PwShop {pwShop.PwShopId} has been disabled - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.UnauthorizedAccessUrl, currentUrl);
                return;
            }

            if (!pwShop.IsBillingValid)
            {
                var billingRepository = factory.MakeBillingRepository(pwShop);
                var charge = billingRepository.RetrieveCurrent();
                AuthConfig.GlobalSignOut(signInManager);

                if (charge.UserNeedsToLoginAgain)
                {
                    logger.Info($"PwShop {pwShop.PwShopId} has invalid Billing - User logging in again");
                    filterContext.Result = GlobalConfig.Redirect(AuthConfig.LoginUrl(pwShop.Domain, currentUrl));
                    return;
                }
                else
                {
                    logger.Info($"PwShop {pwShop.PwShopId} has invalid Billing - User must contact support");                    
                    filterContext.Result = GlobalConfig.Redirect(AuthConfig.BillingProblemUrl, currentUrl);
                    return;
                }
            }

            if (!pwShop.IsAccessTokenValid)
            {
                logger.Info(
                    $"The Access Token for PwShop {pwShop.PwShopId} needs to be Refreshed - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.AccessTokenRefreshUrl, currentUrl);
                return;
            }

            // Retrieve User Access Roles
            var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
            var userRoleIds = user.Roles.Select(x => x.RoleId);
            var userRoles =
                roleManager.Roles
                    .Where(x => userRoleIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToList();

            var identity = new IdentitySnapshot()
            {
                UserId = userId,
                UserName = user.UserName,
                Roles = userRoles,
                Email = user.Email,

                Impersonated = result.Impersonated,
                PwShop = pwShop,
                ShopDomain = result.ShopDomain,
            };

            logger.Debug($"Successfully hydrated User {user.Id} Identity Snapshot into HttpContext");

            var commonContext = new AuthenticatedContext
            {
                Today = timeZoneTranslator.ToOtherTimeZone(DateTime.Today, pwShop.TimeZone),
                ShopifyApiKey = ProfitWiseConfiguration.Settings.ShopifyApiKey,
                IdentitySnapshot = identity,
            };

            filterContext.HttpContext.AuthenticatedContext(commonContext);
        }
    }
}
