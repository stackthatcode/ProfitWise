using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Interfaces;

namespace ProfitWise.Web.Attributes
{
    public class IdentityProcessingAttribute : ActionFilterAttribute, IActionFilter
    {        
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            using (var scope = DependencyResolver.Current.GetService<ILifetimeScope>())
            {
                var dbContext = scope.Resolve<ApplicationDbContext>();
                var roleManager = scope.Resolve<ApplicationRoleManager>();
                var credentialService = scope.Resolve<IShopifyCredentialService>();
                var shopRepository = scope.Resolve<PwShopRepository>();
                var signInManager = scope.Resolve<ApplicationSignInManager>();
                var logger = scope.Resolve<IPushLogger>();

                // Pull the User ID from OWIN plumbing...
                var currentUrl = filterContext.HttpContext.Request.Url.PathAndQuery;
                var userId = filterContext.HttpContext.User.ExtractUserId();


                if (userId == null)
                {
                    logger.Debug($"Null UserId - aborting IdentityProcessing");

                    // It appears that the Authorize Attribute has incorrectly authorized a user...?
                    filterContext.Result = AuthConfig.UnauthorizedAccessRedirect(currentUrl);
                    return;
                }

                var user = dbContext.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null)
                {
                    logger.Error($"Unable to retrieve User with Id {userId} - aborting IdentityProcessing");
                    AuthConfig.GlobalSignOut(signInManager);
                    filterContext.Result = AuthConfig.SevereAuthorizationFailureRedirect(currentUrl);
                    return;
                }

                var result = credentialService.Retrieve(userId);
                if (result.Success == false)
                {
                    // On failure of credential service, throw an error, which will redirect to Server Fault page
                    logger.Error(
                        $"Unable to Retrieve Claims for User {userId}: '{result.Message}' - aborting IdentityProcessing");
                    AuthConfig.GlobalSignOut(signInManager);
                    filterContext.Result = AuthConfig.SevereAuthorizationFailureRedirect(currentUrl);
                    return;
                }

                var pwShop = shopRepository.RetrieveByUserId(userId);
                if (!pwShop.IsShopEnabled)
                {
                    logger.Info(
                        $"PwShop {pwShop.PwShopId} has been disabled - aborting IdentityProcessing");
                    AuthConfig.GlobalSignOut(signInManager);
                    filterContext.Result = AuthConfig.SevereAuthorizationFailureRedirect(currentUrl);
                    return;
                }

                if (!pwShop.IsAccessTokenValid)
                {
                    logger.Info(
                        $"The Access Token for PwShop {pwShop.PwShopId} needs to be Refreshed - aborting IdentityProcessing");
                    AuthConfig.GlobalSignOut(signInManager);
                    filterContext.Result = AuthConfig.AccessTokenRefreshRedirect(currentUrl);
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
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = userRoles,
                    Email = user.Email,

                    Impersonated = result.Impersonated,
                    PwShop = pwShop,
                    ShopDomain = result.ShopDomain,
                };

                logger.Debug($"Successfully hydrated User {user.Id} Identity Snapshot into HttpContext");
                filterContext.HttpContext.PushIdentitySnapshot(identity);
            }
        }
    }
}
