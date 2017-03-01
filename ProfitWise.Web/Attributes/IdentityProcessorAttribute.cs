using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Configuration;
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
            var credentialService = DependencyResolver.Current.GetService<IShopifyCredentialService>();
            var shopRepository = DependencyResolver.Current.GetService<ShopRepository>();
            var signInManager = DependencyResolver.Current.GetService<ApplicationSignInManager>();
            var logger = DependencyResolver.Current.GetService<IPushLogger>();
            var timeZoneTranslator = DependencyResolver.Current.GetService<TimeZoneTranslator>();

            // Pull the User ID from OWIN plumbing...
            var currentUrl = filterContext.HttpContext.Request.Url.PathAndQuery;
            var shopParameter = filterContext.HttpContext.Request.QueryString["shop"];
            var userId = filterContext.HttpContext.User.ExtractUserId();

            if (userId == null)
            {
                logger.Info($"Null UserId - aborting IdentityProcessing");

                // It appears that the Authorize Attribute has incorrectly authorized a user...?
                filterContext.Result = 
                    GlobalConfig.Redirect(AuthConfig.UnauthorizedAccessUrl, currentUrl);
                return;
            }

            var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                logger.Error($"Unable to retrieve User with Id {userId} - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, currentUrl);
                return;
            }

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
            if (pwShop == null)
            {
                // On failure of credential service, throw an error, which will redirect to Server Fault page
                logger.Error(
                    $"Shop does not exist for User {effectiveUserId}: '{result.Message}' - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.SevereAuthorizationFailureUrl, currentUrl);
                return;
            }

            if (shopParameter != null && shopParameter != pwShop.Domain)
            {
                logger.Error($"Currently logged in as {pwShop.Domain}, while accessing App from {shopParameter}");
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

            if (!pwShop.IsAccessTokenValid)
            {
                logger.Info(
                    $"The Access Token for PwShop {pwShop.PwShopId} needs to be Refreshed - aborting IdentityProcessing");
                AuthConfig.GlobalSignOut(signInManager);
                filterContext.Result = GlobalConfig.Redirect(AuthConfig.AccessTokenRefreshUrl, currentUrl);
                return;
            }

            // Retrieve User Access Roles
            var userRoleIds = user.Roles.Select(x => x.RoleId);
            var userRoles =
                roleManager.Roles
                    .Where(x => userRoleIds.Contains(x.Id))
                    .Select(x => x.Name)
                    .ToList();

            var identity = new IdentitySnapshot()
            {
                UserId = effectiveUserId,
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
